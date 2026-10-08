// Recovered compatibility source; provenance: recovered-support.json.
package sc.app.stc.utils;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.util.concurrent.CopyOnWriteArrayList;

public class DataCallBackUtils {
    private static Logger logger = LoggerFactory.getLogger(DataCallBackUtils.class);
    private static CopyOnWriteArrayList<IDataCallBack> callBacks = new CopyOnWriteArrayList();

    public static void notifyCallBack(DataType dataType) {
        logger.info(" notify {}", (Object) dataType);
        for (IDataCallBack iDataCallBack : callBacks) {
            try {
                iDataCallBack.OnCallBack(dataType);
            } catch (Exception e) {
                logger.error("notifyCallBack error.", (Throwable) e);
            }
        }
    }

    public static void addCallBack(IDataCallBack callBack) {
        logger.info(" addCallBack {}", (Object) callBack);
        callBacks.add(callBack);
    }

    public static void removeCallBack(IDataCallBack callBack) {
        logger.info(" removeCallBack {}", (Object) callBack);
        callBacks.remove(callBack);
    }
}
