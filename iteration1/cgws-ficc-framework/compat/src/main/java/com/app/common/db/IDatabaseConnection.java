// Recovered compatibility source; provenance: recovered-support.json.
package com.app.common.db;

import java.sql.Connection;

public interface IDatabaseConnection {
    public Connection getConnection();

    public void freeConnection(Connection var1);
}
