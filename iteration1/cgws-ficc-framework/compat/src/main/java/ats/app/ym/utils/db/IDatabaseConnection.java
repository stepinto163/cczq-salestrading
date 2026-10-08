// Recovered compatibility source; provenance: recovered-support.json.
package ats.app.ym.utils.db;

import java.sql.Connection;

public interface IDatabaseConnection {
    public Connection getConnection();

    public void freeConnection(Connection var1);
}
