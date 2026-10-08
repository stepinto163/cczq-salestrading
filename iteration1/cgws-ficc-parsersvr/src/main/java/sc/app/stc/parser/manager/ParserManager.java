package sc.app.stc.parser.manager;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import cn.hutool.core.map.multi.RowKeyTable;
import cn.hutool.core.map.multi.Table;
import cn.hutool.core.util.ReflectUtil;
import cn.hutool.core.util.StrUtil;
import cn.hutool.core.util.XmlUtil;
import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.model.BondQuote;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.springframework.util.CollectionUtils;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;
import sc.app.stc.api.base.po.BondInfoView;
import sc.app.stc.fix.MessageDictionary;
import sc.app.stc.parser.po.ParsePo;
import sc.app.stc.parser.po.TableIntentionQuote;
import sc.app.stc.parser.stream.CommonDataManager;
import sc.app.stc.parser.utils.Consts;
import sc.app.stc.parser.utils.DateUtil8;
import sc.app.stc.parser.utils.HanlpUtils;
import sc.app.stc.parser.utils.FiccCrmClient;
import sc.app.stc.utils.IdUtil;
import sc.app.stc.utils.TextUtil;
import sc.app.stc.utils.WeekDateSettlement;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

public class ParserManager {
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(ParserManager.class);
    private static final Table<String, String, String> SYNONYM_TABLE = new RowKeyTable<>();
    //清算速度
    private static final WeekDateSettlement wds = new WeekDateSettlement();

    public static void init() {
        //初始化同义词典
        Document doc = XmlUtil.readXML(HanlpUtils.HANLP_ROOT + "/data/dictionary/SynonymsDictionary.xml");
        Element element = doc.getDocumentElement();
        List<Element> synonyms = XmlUtil.getElements(element, "Synonyms");
        synonyms.forEach(item -> {
            String wordType = item.getAttribute("wordType");
            List<Element> synonymList = XmlUtil.getElements(item, "Synonym");
            synonymList.forEach(synonym -> {
                String keyWord = synonym.getAttribute("keyWord");
                List<Element> synonymWords = XmlUtil.getElements(synonym, "Word");
                synonymWords.forEach(synonymWord -> {
                    String nodeValue = synonymWord.getTextContent();
                    SYNONYM_TABLE.put(wordType, nodeValue, keyWord);
                });
            });
        });
    }

    /**
     * 获取同义词的主词
     *
     * @param wordType
     * @param word
     * @return
     */
    public static String getSynonymKeyWord(String wordType, String word) {
        if (StrUtil.isBlank(wordType) || StrUtil.isBlank(word)) {
            return word;
        }
        String keyWord = SYNONYM_TABLE.get(wordType, word);
        if (StrUtil.isNotBlank(keyWord)) {
            return keyWord;
        } else {
            return word;
        }
    }


    public static List<TableIntentionQuote> convertIntentionQuote(AllQuote allQuote, ParsePo req) {
        List<TableIntentionQuote> tableIntentionQuoteList = new ArrayList<>();
        for (BondQuote bondQuote : allQuote.lstQuote) {
            TableIntentionQuote intentionQuote = new TableIntentionQuote();
            intentionQuote.setIsInternal(bondQuote.IsInternal);
            intentionQuote.setClordid(IdUtil.getId());
            intentionQuote.setStatus("0");
            intentionQuote.setSecurityId(bondQuote.Code.toUpperCase());
            if (StringUtils.isNotBlank(bondQuote.Name)) {
                intentionQuote.setSymbol(bondQuote.Name.replace(" ", "").toUpperCase());
            }
            intentionQuote.setSide(translateSide(bondQuote.Direction, bondQuote.Text));
            intentionQuote.setSettlType(translateSettleType(bondQuote.settlementDay, bondQuote.tradeDay, bondQuote.Text));
            intentionQuote.setPrice(StrUtil.isBlank(bondQuote.Price) ? null : new BigDecimal(bondQuote.Price));
            intentionQuote.setQty(StrUtil.isBlank(bondQuote.Volume) ? null : new BigDecimal(bondQuote.Volume));
            intentionQuote.setText(bondQuote.Text);
            intentionQuote.setUserName(req.getUserName());
            String time = TextUtil.getFormatCurrentTimeToMilSecond();
            intentionQuote.setCreateTime(time);
            intentionQuote.setUpdateTime(time);
            intentionQuote.setCloseBy(req.getUserName());
            intentionQuote.setParseTraderIdbkey(allQuote.trader);
            intentionQuote.setParseCompanyIdbkey(allQuote.company);
            intentionQuote.setPriceType(translatePriceType(bondQuote.priceType));
            if (null != allQuote.company) {
                TBrokerCrmInst crmInst = FiccCrmClient.getInstByIDBkey(allQuote.company);
                intentionQuote.setInstitutionId(crmInst == null ? null : crmInst.idbkey);
                intentionQuote.setInstitutionName(crmInst == null ? allQuote.company : crmInst.shortnamecn);
            }
            if (null != allQuote.trader) {
                TBrokerCrmContact crmContact = FiccCrmClient.getContact(allQuote.trader);
                intentionQuote.setTraderId(crmContact == null ? null : crmContact.idbkey);
                intentionQuote.setTraderName(crmContact == null ? allQuote.trader : crmContact.namecn);
            }
            tableIntentionQuoteList.add(intentionQuote);

            BondInfoView bondInfoView = CommonDataManager.getBondInfo(bondQuote.Code.toUpperCase());
            if(null != bondInfoView){
                intentionQuote.setTimeToMaturity(bondInfoView.TimeToMaturity);
                intentionQuote.setTimeToMaturityToWorkDay(bondInfoView.timeToMaturityToWorkDay);
                intentionQuote.setSign(bondInfoView.sign);
                intentionQuote.setSign1(bondInfoView.sign1);
                intentionQuote.setSign2(bondInfoView.sign2);
                intentionQuote.setSign3(bondInfoView.sign3);
                getExtraParams(intentionQuote,bondInfoView);
            }
        }
        return tableIntentionQuoteList;
    }

    /**
     * 补充订单信息
     *
     * @param t
     * @param bondInfo
     * @param <T>
     */
    public static <T> void getExtraParams(T t, BondInfoView bondInfo) {
        if (t == null || bondInfo == null) {
            return;
        }
        Map<String, Object> paramMap = CommonDataManager.getExtraParams(bondInfo);
        //遍历反射赋值
        for (Map.Entry<String, Object> entry : paramMap.entrySet()) {
            String key = entry.getKey();
            Object value = entry.getValue();
            if (ReflectUtil.hasField(t.getClass(), key)) {
                try {
                    ReflectUtil.setFieldValue(t, key, value);
                } catch (Exception e) {
                    logger.error("getExternalParam error", e);
                }
            }
        }
    }

    /**
     * idb价格类型【0：收益率； 1：净价；2：全价】
     * 转化为：净价:"1" 全价:"2",收益率:"3"
     */
    private static String translatePriceType(int priceType) {
        switch (priceType){
            case 0:
                return "3";
            case 1:
                return "1";
            case 2:
                return "2";
            default:
                return "3";
        }
    }


    /**
     * 方向处理，默认ofr
     */
    public static String translateSide(String originSide, String originText) {
        String[] textArray = originText.trim().split(" ");
        for (String text : textArray) {
            if (Consts.BID_CHINESE.contains(text)) {
                return "1";
            } else if (Consts.OFR_CHINESE.contains(text)) {
                return "2";
            }
        }
        return "bid".equalsIgnoreCase(originSide) ? "1" : "2";
    }

    /**
     * 清算类型处理，默认T1
     */
    public static String translateSettleType(String originType, String originDay, String originText) {
        //如果识别服务识别到清算类型，按照识别到的进行处理
        if (StringUtils.isNotEmpty(originType)) {
            if (StringUtils.isNotEmpty(originDay)) {
                String settlText = originDay + originType;
                settlText = calSettleType(settlText);
                return settlText.contains(".") ? settlText : containSettleType(settlText);
            }
            return containSettleType(originType);
        } else {
            //根据原文计算清算速度
            String settlText = calSettleType(originText);
            if (StringUtils.isEmpty(settlText)) {
                return containSettleType(originText);
            } else {
                return settlText.contains(".") ? settlText : containSettleType(settlText);
            }
        }
    }

    /**
     * 根据原文计算清算速度
     */
    public static String calSettleType(String settlText) {
        wds.setBaseDate(DateUtil8.getNowTime("yyyyMMdd"));
        List<String> settlList = wds.calTod0Tod1AndAll2("", "", settlText);
        if (!CollectionUtils.isEmpty(settlList)) {
            return settlList.get(0);
        }
        return "";
    }

    /**
     * 做原文是否包含的识别
     */
    public static String containSettleType(String originType) {
        if (originType.contains("明天")) {
            return originType.contains("+0") ? MessageDictionary.FieldsEnum.SettlType.Tom0 :
                    MessageDictionary.FieldsEnum.SettlType.Tom1;
        }
        return originType.contains("+0") ? MessageDictionary.FieldsEnum.SettlType.T0 :
                MessageDictionary.FieldsEnum.SettlType.T1;
    }

    /**
     * 价格处理，扩展识别
     */
    public static BigDecimal translatePrice(String originPrice, String originText, String securityId) {
        logger.info("translatePrice originPrice:{}", originPrice);
        if (StringUtils.isNotEmpty(originPrice)) {
            //识别价格不带小数，并且价格超过两位数；根据中债估值处理
            if (!originPrice.contains(".") && originPrice.length() > 1) {
                return getPropertyPrice(originPrice, securityId);
            }
            return new BigDecimal(originPrice);
        }
        String[] textArray = originText.trim().split(" ");
        for (String text : textArray) {
            //去除%字符
            if (text.contains("%")) {
                text = text.replace("%", "");
            }
            //根据中债估值处理
            if (text.length() < 4 && StringUtils.isNumeric(text)) {
                return getPropertyPrice(text, securityId);
            }
        }
        return null;
    }

    /**
     * 根据中债估值获取合适的收益率价格
     */
    public static BigDecimal getPropertyPrice(String originPrice, String securityId) {
        BigDecimal yieldNum = CommonDataManager.getCbv(securityId);
        if (null == yieldNum) {
            return null;
        }
        //获取收益率整数部分
        int valuation = yieldNum.intValue();
        //比较那个数字接近中债估值
        BigDecimal bigNum = new BigDecimal(valuation + 1 + "." + originPrice);
        BigDecimal smallNum = new BigDecimal(valuation + "." + originPrice);
        BigDecimal bigDiff = bigNum.subtract(yieldNum).abs();
        BigDecimal smallDiff = yieldNum.subtract(smallNum).abs();
        return bigDiff.compareTo(smallDiff) > 0 ? smallNum : bigNum;
    }

    /**
     * 量处理，是否整量：能被100整除，小于10亿
     */
    public static BigDecimal translateQty(String originQty, String originText) {
        if (StringUtils.isNumeric(originQty)) {
            long qty = Long.parseLong(originQty);
            if (qty % 100 != 0 || qty > 100000) {
                return null;
            }
            return new BigDecimal(originQty);
        }
        //根据符号"/"分割，补充识别
        String[] textArray = originText.trim().split(" ");
        for (String text : textArray) {
            String[] array = text.split("/");
            for (String data : array) {
                if (StringUtils.isNumeric(data)) {
                    long qty = Long.parseLong(data);
                    if (qty % 100 != 0 || qty > 100000) {
                        return null;
                    }
                    return new BigDecimal(data);
                }
            }
        }
        return null;
    }

}
