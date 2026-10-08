package sc.app.stc.parser.stream;

import ats.app.api.BaseApi.IMessage;
import ats.bbox.framework.EDataException;
import ats.bbox.framework.LifecycleException;
import ats.bbox.framework.Server;
import ats.bbox.framework.stream.StreamAdapter;
import ats.foundation.utils.exception.CodeException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import sc.app.stc.api.HolidyResult;
import sc.app.stc.net.NetClientResource;
import sc.app.stc.utils.BaseSvrClient;
import sc.app.stc.utils.Consts;
import sc.app.stc.utils.TextUtil;
import sc.app.stc.utils.Utils;

import java.util.Map;

/**
 * 初始化BaseSvr基础数据
 *
 * @author deshuai.kong
 */
public class BaseStream extends StreamAdapter {
    private Logger logger = LoggerFactory.getLogger(BaseStream.class);
    public NetClientResource cedaClientResource;

    @Override
    public void dispose() throws LifecycleException {

        super.dispose();
    }

    @Override
    public void init() throws LifecycleException {
        super.init();
        cedaClientResource = (NetClientResource) Server.getServer().getResource("NetClientResource");
    }

    @Override
    public void onData(Map<String, Object> arg0) throws EDataException {
        super.onData(arg0);
    }

    IMessage<HolidyResult> message = (topic, t) -> {
        logger.info("on holiday day end message day end start");
        try {
            Consts.DBType = Utils.getDBType(CommonDataManager.dbPoolResource);
            CommonDataManager.clearCache();
            TextUtil.clear();// 清理清算类型缓存
            logger.info("on holiday day end message day end end");
        } catch (Exception e) {
            logger.error("on holiday day end message day end error", e.getMessage(), e);
        }

    };

    @Override
    public void start() throws LifecycleException {

        super.start();
        if (getStatus() != STATUS.STARTED) {
            logger.warn("ParseSvr does not start");
            return;
        }
        try {
            //初始化债券数据
            BaseSvrClient.isLoadBond = true;
            //初始化节假日数据
            BaseSvrClient.isLoadAllHoliday = true;
            //初始化中债估值
            BaseSvrClient.isLoadChianBond = true;
            //订阅债券基础信息
            //BaseSvr2Client.isReloadBondInfos = true;
            //连接baseSvr获取数据
            BaseSvrClient.initialize(cedaClientResource.getNetClientWrapper(), message);
        } catch (CodeException e) {
            logger.error("BaseSvrClient init error", e);
        }
    }

    @Override
    public void stop() throws LifecycleException {

        super.stop();

    }

}
