package sc.app.stc.parser.manager;

import cn.hutool.core.collection.CollUtil;
import cn.hutool.core.util.NumberUtil;
import cn.hutool.core.util.StrUtil;
import cn.hutool.json.JSONUtil;
import com.hankcs.hanlp.corpus.tag.Nature;
import com.hankcs.hanlp.seg.common.Term;
import org.junit.Before;
import org.junit.Test;
import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.parser.stream.CommonDataManager;
import sc.app.stc.parser.utils.HanlpUtils;
import sc.app.stc.utils.BaseSvr2Client;

import java.math.BigDecimal;
import java.math.RoundingMode;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class IRSParserManager extends ParserManager {

    public static final String BOND_ID = "BondID";
    public static final String TIME_TO_MATURITY = "TimeToMaturity";
    public static final String SIDE = "side";
    public static final String PRICE = "price";
    public static final String QUANTITY = "quantity";
    public static final String PARSE_TYPE = "parseType";
    public static final String ORIGINAL = "original";


    public static List<Map<String, Object>> parseIrs(List<String> content) {

        List<Map<String, Object>> resultList = new ArrayList<>();
        if (CollUtil.isNotEmpty(content)) {
            content.forEach(item -> {
                List<Term> terms = HanlpUtils.parseWithCustomDictionary(item);
                Map<String, Object> map = parse(terms);
                if (CollUtil.isNotEmpty(map)) {
                    //返回原文
                    map.put(ORIGINAL, item);
                    resultList.add(map);
                }

            });
        }
        return resultList;
    }

    /**
     * 一行数据解析
     *
     * @param terms
     * @return
     */
    private static Map<String, Object> parse(List<Term> terms) {
        Map<String, Object> map = new HashMap<>(5);
        for (int i = 0; i < terms.size(); i++) {
            Term term = terms.get(i);
            if ("bond".equals(term.nature.toString())) {
                if (map.containsKey(BOND_ID)) {
                    continue;
                }
                map.put(BOND_ID, getSynonymKeyWord(term.nature.toString(), term.word));
            } else if ("term".equals(term.nature.toString())) {
                if(map.containsKey(TIME_TO_MATURITY)){
                    continue;
                }
                map.put(TIME_TO_MATURITY, getSynonymKeyWord(term.nature.toString(), term.word));
            } else if ("side".equals(term.nature.toString())) {
                if(map.containsKey(SIDE)){
                    continue;
                }
                map.put(SIDE, getSynonymKeyWord(term.nature.toString(), term.word));
            } else if (Nature.m.equals(term.nature)) {
                String word = term.word;
                if (NumberUtil.isNumber(word)) {
                    BigDecimal number = NumberUtil.toBigDecimal(word);
                    //如果数字能够被50整除，那么就是数量
                    if (number.remainder(new BigDecimal(50)).compareTo(BigDecimal.ZERO) == 0) {
                        if(map.containsKey(QUANTITY)){
                            continue;
                        }
                        map.put(QUANTITY, number);
                    } else {
                        if(map.containsKey(PRICE)){
                            continue;
                        }
                        //否则就是价格
                        if (i > 0) {
                            //往前找一位，看价格是否为负
                            Term preTerm = terms.get(i - 1);
                            if (Nature.nx.equals(preTerm.nature) && StrUtil.DASHED.equals(preTerm.word)) {
                                //前一个字符是减号
                                map.put(PRICE, NumberUtil.toBigDecimal(preTerm.word + term.word));
                            } else {
                                map.put(PRICE, number);
                            }
                        } else {
                            map.put(PRICE, number);
                        }

                    }
                }
            } else if ("price".equals(term.nature.toString())) {
                //识别到价格单位，前一个数字就是价格
                for (int j = i - 1; j >= 0; j--) {
                    Term preTerm = terms.get(j);
                    if (Nature.m.equals(preTerm.nature)) {
                        //再往前找一位，看价格是否为负
                        if (j > 0) {
                            Term prePreTerm = terms.get(j - 1);
                            if (Nature.nx.equals(prePreTerm.nature) && StrUtil.DASHED.equals(prePreTerm.word)) {
                                //前一个字符是减号
                                map.put(PRICE, NumberUtil.toBigDecimal(prePreTerm.word + preTerm.word));
                            } else {
                                map.put(PRICE, NumberUtil.toBigDecimal(preTerm.word));
                            }
                        } else {
                            map.put(PRICE, NumberUtil.toBigDecimal(preTerm.word));
                        }
                        break;
                    }
                }
            } else if ("qty".equals(term.nature.toString())) {
                //识别到数量单位，前一个数字就是数量
                for (int j = i - 1; j >= 0; j--) {
                    Term preTerm = terms.get(j);
                    if (Nature.m.equals(preTerm.nature)) {
                        //如果是数量，那么就是50的倍数(50,100,150...
                        BigDecimal number = NumberUtil.toBigDecimal(preTerm.word);
                        if (number.remainder(new BigDecimal(50)).compareTo(BigDecimal.ZERO) == 0) {
                            map.put(QUANTITY, number);
                        }
                        break;
                    }
                }
            }
        }
        //如果没有找到债券，默认是FR007
        if (!map.containsKey(BOND_ID)) {
            map.put(BOND_ID, "FR007");
        }
        if (map.containsKey(TIME_TO_MATURITY)) {
            map.put(BOND_ID, StrUtil.format("{}_{}", map.get(BOND_ID), map.get(TIME_TO_MATURITY)));
        }
        BondInfoView bondInfo = CommonDataManager.getBondInfo(StrUtil.toStringOrNull(map.get(BOND_ID)));
        String priceStr = StrUtil.toStringOrNull(map.get(PRICE));
        BigDecimal price = NumberUtil.toBigDecimal(priceStr);

        if (bondInfo == null) {
            //没有找到债券，债券编码字段留空
            map.remove(BOND_ID);
            //没找到债券信息，价格简单处理
            //价格小于30且带有小数，直接填入
            //价格大于30且带有小数，价格为空
            //价格为整数，价格为空
            if (StrUtil.isNotBlank(priceStr) && priceStr.contains(".")
                    && BigDecimal.valueOf(30.0).compareTo(price) > 0) {
                map.put(PRICE, priceStr);
            } else {
                map.remove(PRICE);
            }
        } else {
            //债券不为空
            if (StrUtil.isBlank(priceStr)) {
                map.remove(PRICE);
            } else if ("Outright".equals(bondInfo.getSECURITY_SUPER_TYPE())) {
                if (priceStr.contains(".")) {
                    //价格带有小数，需要特殊处理
                    if (BigDecimal.valueOf(30).compareTo(price) > 0) {
                        //小于30，直接填入
                        map.put(PRICE, priceStr);
                    } else {
                        //大于30，价格为空
                        map.remove(PRICE);
                    }
                } else {
                    //价格为整数
                    String swapCurveValue = bondInfo.getSwapCurveValue();
                    if (StrUtil.isNotBlank(swapCurveValue)) {
                        //[推算价格] = ROUND (昨日收盘价 - 手输报价*0.01) + 手输报价*0.01
                        BigDecimal val = NumberUtil.toBigDecimal(swapCurveValue);
                        BigDecimal inputPrice = NumberUtil.toBigDecimal("0." + priceStr);
                        BigDecimal finalPrice = NumberUtil.round(NumberUtil.sub(val, inputPrice), 0, RoundingMode.HALF_UP).add(inputPrice);
                        map.put(PRICE, NumberUtil.round(finalPrice, 4, RoundingMode.HALF_UP));
                    } else {
                        //没有昨日收盘价，留空
                        map.remove(PRICE);
                    }
                }
            }
        }

        //返回ParseType
        map.put(PARSE_TYPE, "IRS");

        return map;
    }

    @Before
    public void before() {
        BondInfoView bondInfoView = new BondInfoView();
        bondInfoView.setBond_ID("FR007_3M");
        bondInfoView.setSECURITY_SUPER_TYPE("Outright");
        bondInfoView.setSwapCurveValue("2.345");
        BaseSvr2Client.htBondinfos.put(bondInfoView.Bond_ID, bondInfoView);
        init();
    }

    @Test
    public void test() {
        String[] testCase = new String[]{
                "FR007-1年 50mio 买入 465",
        };
        List<Map<String, Object>> maps = parseIrs(CollUtil.newArrayList(testCase));
        for (Map<String, Object> map : maps) {
            System.out.println(JSONUtil.toJsonStr(map));
        }

    }

    public static void main(String[] args) {
        init();
        String[] testCase = new String[]{
//                "FR007_2Y bid 300mio 2.45",
                "Cfm3: 国泰君安 rec 1X5 37bp in 5y repo for 200 mio",
//                "repo1y 100mio rec 22bp",
//                "cdb10 1y 100mio rec 9945",
//                "d10/g10 1y 100mio rec 99",
//                "1x5 42我还是挂100个bid",
//                "Shibor3M_2Y 150mio rec 99",
        };
        List<Map<String, Object>> maps = parseIrs(CollUtil.newArrayList(testCase));
        for (Map<String, Object> map : maps) {
            System.out.println(JSONUtil.toJsonStr(map));
        }

    }
}
