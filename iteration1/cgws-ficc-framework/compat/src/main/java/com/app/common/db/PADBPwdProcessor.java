// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import com.app.common.cyberark.CyberarkUtil;
import com.app.common.utils.DBConsts;
import com.app.common.utils.StringUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class PADBPwdProcessor implements IDBPwd {
    private Logger logger = LoggerFactory.getLogger(this.getClass());

    @Override
    public String getDBPwd() {
        this.logger.info(
                "UserLocal : {}, SSLVerify: {}, will get db pwd from cyberark",
                (Object) DBConsts.dbPwdType,
                (Object) DBConsts.SSLVerify);
        CyberarkUtil cyberarkUtil = new CyberarkUtil();
        cyberarkUtil.setSSLVerify(DBConsts.SSLVerify);
        String dbPwd =
                cyberarkUtil.getPasswordFromRemote(
                        DBConsts.remoteUrl,
                        DBConsts.appId,
                        DBConsts.safe,
                        DBConsts.folder,
                        DBConsts.object,
                        DBConsts.reason,
                        DBConsts.appKey);
        this.logger.info(
                "read cyberark dbpwd end, length:{}",
                (Object) (StringUtil.isNotEmpty(dbPwd) ? dbPwd.length() : 0));
        return dbPwd;
    }
}
