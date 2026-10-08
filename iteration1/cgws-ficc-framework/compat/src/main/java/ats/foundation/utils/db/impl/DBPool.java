package ats.foundation.utils.db.impl;

import ats.foundation.utils.db.IDBPool;

/**
 * Shares the source-owned STC datasource implementation instead of loading a second pool library.
 */
public final class DBPool extends com.app.common.db.DBPool implements IDBPool {
    private static DBPool instance;

    private DBPool(String datasource, String configuration) throws Exception {
        super(datasource, configuration);
    }

    public static synchronized DBPool instance(String datasource, String configuration)
            throws Exception {
        if (instance == null) {
            instance = new DBPool(datasource, configuration);
        }
        return instance;
    }

    @Override
    public void closeAll() {
        try {
            super.closeAll();
        } finally {
            synchronized (DBPool.class) {
                if (instance == this) {
                    instance = null;
                }
            }
        }
    }
}
