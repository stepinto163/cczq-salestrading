package sc.app.stc.parser.stream;

import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import com.app.common.db.DBPoolResource;
import ats.bbox.framework.stream.StreamAdapter;
import com.alibaba.fastjson.JSON;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.net.NetClientResource;
import sc.app.stc.parser.analyzer.Direction;
import sc.app.stc.parser.analyzer.ICity;
import sc.app.stc.parser.job.ParserCacheJob;
import sc.app.stc.parser.manager.ParserManager;
import sc.app.stc.parser.manager.VirtualParserManager;
import sc.app.stc.parser.po.VirtualPo;
import sc.app.stc.parser.utils.BondCacheHelper;
import sc.app.stc.parser.utils.BondQuoteHelper;
import sc.app.stc.parser.utils.Consts;
import sc.app.stc.parser.utils.DbOperatorUtil;
import sc.app.stc.parser.utils.FiccCrmClient;
import sc.app.stc.parser.utils.ParserInitializationLog;
import sc.app.stc.utils.CommonDataUtils;
import sc.app.stc.utils.Schedule;
import sc.app.stc.utils.Utils;

import java.util.List;

/**
 * TODO
 *
 * @author fangfang.zhang
 * @date 2021/6/8
 */
public class ParserStream extends StreamAdapter {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    private Schedule schedule = new Schedule();

    public NetClientResource netClientResource;

    @Override
    public void init() throws LifecycleException {
        netClientResource = (NetClientResource) Server.getServer().getResource("NetClientResource");
        Consts.ParserEngine = this.getConfig().getString("ParserEngine", "java");
        logger.info("ParserEngine:{}", Consts.ParserEngine);
        Consts.ParserCacheCron = this.getConfig().getString("ParserCacheCron");
        logger.info("ParserCacheCron:{}", Consts.ParserCacheCron);
        Consts.CRMConnSwitch = this.getConfig().getInt("CRMConnSwitch");
        logger.info("CRMConnSwitch:{}", Consts.CRMConnSwitch);
        Consts.CRMServerKey = this.getConfig().getString("CRMServerKey", Consts.CRMServerKey);
        logger.info("CRMServerKey:{}", Consts.CRMServerKey);
        Consts.CRMRequestTimeoutMillis =
                this.getConfig().getInt("CRMRequestTimeoutMillis", Consts.CRMRequestTimeoutMillis);
        logger.info("CRMRequestTimeoutMillis:{}", Consts.CRMRequestTimeoutMillis);
        Consts.IntentionQuoteSize = this.getConfig().getInt("IntentionQuoteSize", Consts.IntentionQuoteSize);
        logger.info("IntentionQuoteSize:{}", Consts.IntentionQuoteSize);
        Consts.ImagePath = this.getConfig().getString("ImagePath", Consts.ImagePath);
        logger.info("ImagePath:{}", Consts.ImagePath);
        Consts.ImageParserType = this.getConfig().getString("ImageParserType", Consts.ImageParserType);
        logger.info("ImageParserType:{}", Consts.ImageParserType);
        Consts.ImageParserApi = this.getConfig().getString("ImageParserApi", Consts.ImageParserApi);
        logger.info("ImageParserApi:{}", Consts.ImageParserApi);
        Consts.ImageParserApiAppId = this.getConfig().getString("ImageParserApiAppId", Consts.ImageParserApiAppId);
        logger.info("ImageParserApiAppId:{}", Consts.ImageParserApiAppId);
        Consts.ImageParserApiSecret = this.getConfig().getString("ImageParserApiSecret", Consts.ImageParserApiSecret);
        logger.info("ImageParserApiSecret:{}", Consts.ImageParserApiSecret);


        Consts.DealParserAuthorization = this.getConfig().getString("DealParserAuthorization", Consts.DealParserAuthorization);
        logger.info("DealParserAuthorization:{}", Consts.DealParserAuthorization);

        Consts.DealParserApi = this.getConfig().getString("DealParserApi", Consts.DealParserApi);
        logger.info("DealParserApi:{}", Consts.DealParserApi);

        int isCyberark = this.getConfig().getInt("isCyberark");
        Consts.isCyberark = isCyberark;
        String remoteUrl = this.getConfig().getString("remoteUrl");
        Consts.remoteUrl = remoteUrl;
        String cyAppId = this.getConfig().getString("appId");
        Consts.appId = cyAppId;
        String safe = this.getConfig().getString("safe");
        Consts.safe = safe;
        String folder = this.getConfig().getString("folder");
        Consts.folder = folder;
        String object = this.getConfig().getString("object");
        Consts.object = object;
        String reason = this.getConfig().getString("reason");
        Consts.reason = reason;
        String appKey = this.getConfig().getString("appKey");
        Consts.appKey = appKey;
        Consts.esgGrantType = this.getConfig().getString("esgGrantType");
        Consts.esgOauthURL = this.getConfig().getString("esgOauthURL");
        Consts.esgClientId = this.getConfig().getString("esgClientId");
        logger.info("----------------------------------------------------------");
        logger.info("isCyberark: " + Consts.isCyberark);
        logger.info("remoteUrl: " + Consts.remoteUrl);
        logger.info("cyAppId: " + Consts.appId);
        logger.info("safe: " + Consts.safe);
        logger.info("folder: " + Consts.folder);
        logger.info("object: " + Consts.object);
        logger.info("reason: " + Consts.reason);
        logger.info("appKey: " + Consts.appKey);
        logger.info("esgGrantType: " + Consts.esgGrantType);
        logger.info("esgOauthURL: " + Consts.esgOauthURL);
        logger.info("esgClientId: " + Consts.esgClientId);

        logger.info("cyberark end----------------------------------------------------------");
        
        DBPoolResource dbPoolResource = (DBPoolResource) Server.getServer().getResource(
                "DBPoolResource");//.getConfig().getString("dbpool.default"); =
        Consts.dbpoolDefault = dbPoolResource.getConfig().getString("dbpool.default");
        logger.info("dbpoolDefault:{}", Consts.dbpoolDefault);
        CommonDataUtils.dbPoolResource = (DBPoolResource) Server.getServer().getResource("DBPoolResource");
        Consts.DBType = Utils.getDBType(CommonDataManager.dbPoolResource);
        logger.info("DBType:{}", Consts.DBType);
        CommonDataUtils.init();
        ParserManager.init();
        Direction.init();
        ICity.initProvinceInfo();
    }
    private void Cron() {
        //调度初始化
        schedule.init(getConfig());
        //调度更新
        schedule.updateJob(Consts.ParserCacheCron, "day", ParserCacheJob.class,true);
        //调度开始
        schedule.start();
    }
    @Override
    public void start() throws LifecycleException {
        if ("native".equalsIgnoreCase(Consts.ParserEngine)) {
            DbOperatorUtil.load();
        }
        try {
            if (Consts.CRMConnSwitch == 1) {
                // CRM comes from CRMSvr: one bounded lookup per parsed name, no local CRM mirror.
                FiccCrmClient.initialize(
                        netClientResource.getNetClientWrapper(),
                        Consts.CRMServerKey,
                        Consts.CRMRequestTimeoutMillis);
            }
            // The selected engine loads one complete dictionary snapshot during construction.
            BondQuoteHelper.getInstance();
            ParserInitializationLog.run("BOND_CACHE", "BondCacheHelper", "MYSQL0",
                    () -> BondCacheHelper.loadData());
            Cron();
        } catch (Exception e) {
            logger.error("ParserSvr initialization failed", e);
            throw new IllegalStateException("ParserSvr initialization failed", e);
        }
    }

    @Override
    public void stop() throws LifecycleException {
        BondQuoteHelper.closeEngine();
        super.stop();
        schedule.stop();
        ICity.clear();
        Direction.clear();
    }

    public void ParseTest(){

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
            List<VirtualPo> parser = VirtualParserManager.parser(content);
            System.out.println(content+ "---"+ JSON.toJSONString(parser));
        }

    }
}
