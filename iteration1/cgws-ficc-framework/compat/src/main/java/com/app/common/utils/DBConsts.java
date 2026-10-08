// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.utils;

import ats.foundation.utils.config.ATSROOTConfig;
import ats.foundation.utils.config.PropertiesHelper;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class DBConsts {
    private static Logger logger = LoggerFactory.getLogger(DBConsts.class);
    public static boolean SSLVerify = false;
    public static String dbPwdType = "";
    public static String remoteUrl;
    public static String appId;
    public static String appKey;
    public static String safe;
    public static String folder;
    public static String object;
    public static String reason;

    static {
        logger.info("load DBConsts");
        PropertiesHelper atsrootConfig = ATSROOTConfig.getATSROOTConfig();
        String dbPwdType = atsrootConfig.getProperty("dbPwdType");
        String remoteUrl = atsrootConfig.getProperty("remoteUrl");
        String appId = atsrootConfig.getProperty("appId");
        String appKey = atsrootConfig.getProperty("appKey");
        String safe = atsrootConfig.getProperty("safe");
        String folder = atsrootConfig.getProperty("folder");
        String object = atsrootConfig.getProperty("object");
        String reason = atsrootConfig.getProperty("reason");
        String sslVerify = atsrootConfig.getProperty("SSLVerify");
        if (StringUtil.isNotEmpty(sslVerify)) {
            SSLVerify = Boolean.valueOf(sslVerify);
        }
        DBConsts.dbPwdType = dbPwdType;
        DBConsts.remoteUrl = remoteUrl;
        DBConsts.appId = appId;
        DBConsts.appKey = appKey;
        DBConsts.safe = safe;
        DBConsts.folder = folder;
        DBConsts.object = object;
        DBConsts.reason = reason;
        logger.info("dbPwdType:{}", (Object) dbPwdType);
        logger.info("remoteUrl:{}", (Object) remoteUrl);
        logger.info("appId:{}", (Object) appId);
        logger.info("appKey:{}", (Object) appKey);
        logger.info("safe:{}", (Object) safe);
        logger.info("folder:{}", (Object) folder);
        logger.info("object:{}", (Object) object);
        logger.info("reason:{}", (Object) reason);
        logger.info("SSLVerify:{}", (Object) SSLVerify);
    }
}
