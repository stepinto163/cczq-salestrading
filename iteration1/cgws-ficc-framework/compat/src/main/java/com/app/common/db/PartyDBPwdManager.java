// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import com.app.common.utils.DBConsts;
import com.app.common.utils.StringUtil;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

public class PartyDBPwdManager {
    private static Logger logger = LoggerFactory.getLogger(DBConsts.class);
    private static PartyDBPwdManager partyDBPwdManager = new PartyDBPwdManager();

    public static PartyDBPwdManager getInstance() {
        return partyDBPwdManager;
    }

    private PartyDBPwdManager() {}

    public String getPartyDBPwd() {
        String partyDBPwd = "";
        logger.info("initDBPool dbPwdType:{}", (Object) DBConsts.dbPwdType);
        if (StringUtil.isNotEmpty(DBConsts.dbPwdType)) {
            switch (DBConsts.dbPwdType) {
                case "1":
                    {
                        PADBPwdProcessor dbPwdProcessor = new PADBPwdProcessor();
                        partyDBPwd = dbPwdProcessor.getDBPwd();
                        break;
                    }
            }
        }
        logger.info("partyDBPwd end");
        return partyDBPwd;
    }
}
