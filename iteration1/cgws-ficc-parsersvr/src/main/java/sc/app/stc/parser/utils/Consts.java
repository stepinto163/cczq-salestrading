package sc.app.stc.parser.utils;

import java.util.ArrayList;
import java.util.List;

public class Consts extends sc.app.stc.utils.Consts  {
    public static String VERSION = "v1.0.0 (build20190227)";
    public static String dbpoolDefault = "MYSQL0";
    public static String ParserEngine = "java";
    public static String ParserCacheCron="";

    public static int CRMConnSwitch;
    // CRM 服务在总线上的服务名（FICC CRMSvr）
    public static String CRMServerKey = "SERVER.CRMSvr";
    // CRM 查询超时（毫秒）
    public static int CRMRequestTimeoutMillis = 3000;
    //最大识别意向报价条数
    public static int IntentionQuoteSize = 200;
    // 图片保存地址
    public static String ImagePath = "../../data/ParserSvr/image/";
    public static String ImageParserType = "Sumscope";
    public static String ImageParserApi = "https://www-nn.idbhost.com/open/api/invoke";
    public static String ImageParserApiAppId = "m2u3gn5try";
    public static String ImageParserApiSecret = "";
    // 图片后缀
    public static String ImageSuffix = ".png";
    // 图片内容解析输出文件后缀
    public static String ImageContentSuffix = ".txt";
    public static final String query_bond = "select Bond_Id, Short_name, Bond_Key, listed_market, Delisted_Date from bond_list_info where Delisted_Date > ?;";
    public static final String query_bond_pg = "select Bond_Id, Short_name, Bond_Key, listed_market, Delisted_Date from bond_list_info where Delisted_Date > CAST(? AS INT);";

    //买入中文识别枚举
    public final static List<String> BID_CHINESE = new ArrayList<String>(3) {{
        add("进");
        add("买");
        add("收");
    }};
    //卖出中文识别枚举
    public final static List<String> OFR_CHINESE = new ArrayList<String>(3) {{
        add("出");
        add("卖");
        add("给");
    }};

    public static String DealParserAuthorization = "";

    public static String DealParserApi = "https://finq.qmhost1.com/ai_trade_recog/api/solution-message";

    //# cyberark
    public static Integer isCyberark = 0;
    public static String remoteUrl = "";
    public static String appId = "";
    public static String safe = "";
    public static String folder = "";
    public static String object = "";
    public static String reason = "";
    public static String appKey = "";

    //ESG
    public static String esgClientSecret = "";
    //public static String TOKEN_URL = "https://oauth.egis-optim-dev.paic.com.cn/oauth/token";
    public static String esgOauthURL = "";
    public static String esgClientId = "";
    public static String esgGrantType = "";

}
