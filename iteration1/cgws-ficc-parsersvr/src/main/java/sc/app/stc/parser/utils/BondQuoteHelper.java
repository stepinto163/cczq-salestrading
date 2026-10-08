package sc.app.stc.parser.utils;

import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import ats.message2.CharsetConvertType;
import ats.message2.MsgHelper;
import cgws.ficc.parser.parse.BondQuoteEngine;
import java.time.LocalDate;
import com.protobuf.Parsing;
import cgws.ficc.parser.model.AllQuote;
import cgws.ficc.parser.model.BondQuote;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.springframework.util.CollectionUtils;
import sc.app.stc.api.TBrokerCrmContact;
import sc.app.stc.api.TBrokerCrmInst;
import sc.app.stc.parser.po.BondInfo;

import java.util.*;
import java.util.stream.Collectors;

/**
 * 识别单例
 *
 * @author lemon
 **/
public class BondQuoteHelper {
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(BondQuoteHelper.class);

    private volatile static BondQuoteHelper instance;

    private final BondQuoteEngine bondQuoteParser;

    private BondQuoteHelper() {
        bondQuoteParser = ParserEngineFactory.create(Consts.ParserEngine);
    }

    BondQuoteHelper(BondQuoteEngine engine) {
        if (engine == null) {
            throw new IllegalArgumentException("Parser engine is required");
        }
        bondQuoteParser = engine;
    }

    public static BondQuoteHelper getInstance() {
        if (instance == null) {
            synchronized (BondQuoteHelper.class) {
                if (instance == null) {
                    instance = new BondQuoteHelper();
                }
            }
        }
        return instance;
    }

    public synchronized void loadData() {
        if (bondQuoteParser != null) {
            ParserInitializationLog.run("PARSER_DICTIONARY_REFRESH", bondQuoteParser.getClass().getSimpleName(),
                    "configured PostgreSQL bond dictionary", () -> bondQuoteParser.reload());
        }
    }

    public static void closeEngine() {
        BondQuoteHelper current = instance;
        if (current != null && current.bondQuoteParser instanceof ComparativeBondQuoteEngine) {
            ((ComparativeBondQuoteEngine) current.bondQuoteParser).close();
        }
    }

    public AllQuote parsingAll(String text) {
        try{
            logger.info("===parsingAll:{}", text);
            if (bondQuoteParser != null) {
                LocalDate date = LocalDate.now();
                AllQuote allQuote = bondQuoteParser.parse(text, date);
                logger.info("allQuote:{}", MsgHelper.Serializer(allQuote, CharsetConvertType.None));
                logger.info("trader:{},company:{},traderSuffix:{}",allQuote.trader,allQuote.company,allQuote.traderSuffix);
                if(Consts.CRMConnSwitch==1 && (StringUtils.isNotEmpty(allQuote.trader) || StringUtils.isNotEmpty(allQuote.company))){
                    Parsing.QuoteRsp.Builder quoteResp = Parsing.QuoteRsp.newBuilder();
                    quoteResp.setTrader(allQuote.trader);
                    quoteResp.setCompany(allQuote.company);
                    if(null != allQuote.traderSuffix){
                        quoteResp.setTraderSuffix(allQuote.traderSuffix);
                    }
                    TBrokerCrmInst crmInst = null;//机构
                    List<TBrokerCrmContact> crmContactList = null;//交易员
                    String companyName = allQuote.company;
                    String traderName = allQuote.trader;
//                    allQuote.trader="";
//                    allQuote.company="";
                    if(StringUtils.isNotEmpty(companyName)){
                        crmInst = FiccCrmClient.getInstByName(companyName.trim());
                        logger.info("companyName:{},crmInst:{} ",companyName,crmInst);
                    }
                    if(StringUtils.isNotEmpty(traderName)){
                        crmContactList = FiccCrmClient.getContactByNameMatch(traderName.trim());
                        logger.info("traderName:{},crmContactList:{} ",traderName,crmContactList);
                    }

                    if((quoteResp.hasTrader() && !traderName.isEmpty()) || (quoteResp.hasCompany() && !companyName.isEmpty())){
                        String traderSuffix = quoteResp.hasTraderSuffix() ? allQuote.traderSuffix : null;
                        if(StringUtils.isNotEmpty(traderSuffix)) {
                            traderName = traderName + allQuote.traderSuffix;
                        }
                        if(StringUtils.isEmpty(traderName) || CollectionUtils.isEmpty(crmContactList)){
                            if (StringUtils.isNotEmpty(companyName) && null != crmInst) {
                                allQuote.company = crmInst.idbkey;
                            }
                        } else if(!CollectionUtils.isEmpty(crmContactList)) {

                            Map<String,Double> matchValueMap = new HashMap<>();
                            for(TBrokerCrmContact contact : crmContactList){
                                matchValueMap.put(contact.idbkey,
                                        getMatchValue(companyName,null == crmInst ? null : crmInst.shortnamecn)
                                                + getMatchValue(traderName,null == contact ? null :contact.namecn)
                                                + getMatchValue(contact.instid,null == crmInst ? null : crmInst.idbkey)
                                );
                            }
                            logger.info("matchValueMap:{}",matchValueMap);
                            Map<String, Double> sortMap = new LinkedHashMap<>();
                            matchValueMap.entrySet().stream().sorted((o1, o2) -> o2.getValue().compareTo(o1.getValue()))
                                    .forEach(e -> sortMap.put(e.getKey(), e.getValue()));
                            logger.info("sortMap:{}",sortMap);
                            if(sortMap.size()>0) {
                                Optional<String> first = sortMap.keySet().stream().findFirst();
                                allQuote.trader = first.get();
                                List<TBrokerCrmContact> collect = crmContactList.stream().filter(contact -> contact.idbkey.equals(first.get())).collect(Collectors.toList());
                                allQuote.company = collect.get(0).instid;
                            }
                        }
                    }
                }

                List<BondQuote> quoteList = allQuote.lstQuote;
                List<BondQuote> newList = new ArrayList<>();
                if (!CollectionUtils.isEmpty(quoteList)) {
                    for (BondQuote quote : quoteList) {
                        if (StringUtils.isEmpty(quote.Code)) {
                            continue;
                        }
                        quote.IsInternal = true;
                        String bondCode = quote.Code.toUpperCase();
                        logger.info("parsingExtra bondCode:{}", bondCode);
                        parsingExtra(bondCode, quote, newList);
                    }
                    quoteList.addAll(newList);
                }

                return allQuote;
            }
        } catch (Exception e){
            logger.error("parsingAll error", e);
            throw new IllegalStateException("Quote parsing failed", e);
        }
        return null;
    }


    public static double getMatchValue(String str1, String str2)
    {
        if(StringUtils.isEmpty(str1) || StringUtils.isEmpty(str2))
            return 0.0;
        if(str1.equals(str2)) return 1.0;

        if(str1.length() > str2.length())
            return getMatchValue(str2, str1);

        Set<Character> set = new HashSet<>();
        for(int i = 0; i < str2.length(); i++) {
            set.add(str2.charAt(i));
        }

        int c = 0;
        for(int i = 0; i < str1.length(); i++) {
            if(set.contains(str1.charAt(i))) {
                c++;
            }
        }
        return Double.valueOf(c)/str2.length();
    }


    /**
     * 扩展识别
     */
    private void parsingExtra(String bondCode, BondQuote quote,
                              List<BondQuote> newList) {
        //扩展SH、SZ识别
        /**
         * 对于一个债券代码在银行间和交易所都存在的情况
         * 若带着.sh 和 .sz 就识别成交易所的债券
         * 若带着.IB或者不带后缀，就识别成银行间的债券
         */
//        if (bondCode.contains(".IB")) {
//            //匹配sh债券
//            BondInfo shBond = BondCacheHelper.matchShBond(quote.Text);
//            add2NewList(shBond, quote, newList);
//            //匹配sz债券
//            BondInfo szBond = BondCacheHelper.matchSzBond(quote.Text);
//            add2NewList(szBond, quote, newList);
//        } else
        if (bondCode.contains(".SH")) {
            //匹配sz债券
            BondInfo szBond = BondCacheHelper.matchSzBond(quote.Text);
            add2NewList(szBond, quote, newList);
        } else if (bondCode.contains(".SZ")) {
            //匹配sh债券
            BondInfo shBond = BondCacheHelper.matchShBond(quote.Text);
            add2NewList(shBond, quote, newList);
        }
    }

    /**
     * 识别到sh、sz的债券，加入识别结果
     */
    private void add2NewList(BondInfo bondInfo, BondQuote quote,
                             List<BondQuote> newList) {
        if (StringUtils.isNotEmpty(bondInfo.getBond_Id())) {
            BondQuote newQuote = (BondQuote) quote.clone();
            newQuote.Code = bondInfo.getBond_Id();
            newQuote.Name = bondInfo.getShort_Name();
            newQuote.IsInternal = false;
            newList.add(newQuote);
        }
    }
}
