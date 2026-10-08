package sc.app.stc.parser.job;

import org.quartz.Job;
import org.quartz.JobExecutionContext;
import org.quartz.JobExecutionException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.parser.utils.BondCacheHelper;
import sc.app.stc.parser.utils.BondQuoteHelper;

/**
 * TODO
 *
 * @author fangfang.zhang
 * @date 2021/8/18
 */
public class ParserCacheJob implements Job {
    private Logger logger = LoggerFactory.getLogger(this.getClass());
    @Override
    public void execute(JobExecutionContext jobExecutionContext) throws JobExecutionException {
        try {
            logger.info("reload start");
            BondQuoteHelper.getInstance().loadData();
            BondCacheHelper.loadData();//加载缓存债券
        } catch (Exception e) {
            logger.error("reload error", e);
        }

        logger.info("reload end");
    }
}
