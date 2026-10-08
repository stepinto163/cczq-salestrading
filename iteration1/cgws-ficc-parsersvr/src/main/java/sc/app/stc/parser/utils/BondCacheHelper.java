package sc.app.stc.parser.utils;

import ats.app.ym.utils.db.DBUtils;
import ats.app.ym.utils.logger.AsyncSlf4jLoggerFactory;
import org.apache.commons.lang3.StringUtils;
import org.slf4j.Logger;
import org.springframework.util.CollectionUtils;
import sc.app.stc.parser.po.BondInfo;

import java.util.HashMap;
import java.util.List;

/**
 * 缓存债券信息，处理SH、SZ债券
 *
 * @author lemon
 **/
public class BondCacheHelper {
    private static final Logger logger = AsyncSlf4jLoggerFactory.getLogger(BondCacheHelper.class);

    private static final HashMap<String, String> shKeyMap = new HashMap<>();

    private static final HashMap<String, String> szKeyMap = new HashMap<>();

    private BondCacheHelper() {
        throw new Error("con not init this class");
    }

    public static void loadData() {
        String nowDate = DateUtil8.getNowDate_yMd();
        String sql = Consts.DBType == 3 ? Consts.query_bond_pg : Consts.query_bond;
        List<BondInfo> dataList = DBUtils.queryList(sql, new Object[]{nowDate}, BondInfo.class);
        if (!CollectionUtils.isEmpty(dataList)) {
            shKeyMap.clear();
            dataList.forEach(item -> {
                String bondId = item.getBond_Id();
                String bondName = item.getShort_Name();
                String bondDate = item.getDelisted_Date();
                logger.info("bond cache:{},{},{}", bondId, bondName, bondDate);
                if (StringUtils.isNotEmpty(bondId)) {
                    if (bondId.contains(".SH")) {
                        shKeyMap.put(bondId, bondName);
                    }
                    if (bondId.contains(".SZ")) {
                        szKeyMap.put(bondId, bondName);
                    }
                }
            });
        }
    }

    /**
     * 匹配SH债券
     */
    public static BondInfo matchShBond(String originText) {
        logger.info("matchShBond param:{}", originText);
        String[] textArray = originText.trim().split(" ");
        BondInfo bondInfo = new BondInfo();
        for (String text : textArray) {
            String realCode = text + ".SH";
            if (shKeyMap.containsKey(realCode)) {
                String shortName = shKeyMap.get(realCode);
                bondInfo.setBond_Id(realCode);
                bondInfo.setShort_Name(shortName);
                break;
            }
        }
        logger.info("matchShBond info:{}", bondInfo);
        return bondInfo;
    }

    /**
     * 匹配SZ债券
     */
    public static BondInfo matchSzBond(String originText) {
        logger.info("matchSzBond param:{}", originText);
        String[] textArray = originText.trim().split(" ");
        BondInfo bondInfo = new BondInfo();
        for (String text : textArray) {
            String realCode = text + ".SZ";
            if (szKeyMap.containsKey(realCode)) {
                String shortName = szKeyMap.get(realCode);
                bondInfo.setBond_Id(realCode);
                bondInfo.setShort_Name(shortName);
                break;
            }
        }
        logger.info("matchSzBond info:{}", bondInfo);
        return bondInfo;
    }
}
