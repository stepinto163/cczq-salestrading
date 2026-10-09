package ficc.crm;

import org.apache.log4j.Logger;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

/**
 * 从配置指向的 CRM schema 只读加载机构、联系人及其关联表，形成 CrmDataset。
 * 在通过 CRMSvr 访问 CRM 的部署路径中，表名和字段映射集中在此，消费方使用 CrmDirectory 协议。
 * load 是启动快照读取，不是逐次下拉搜索时访问数据库，也不会写回 CRM 主数据。
 */
public final class CrmStore implements AutoCloseable {
    private static final Logger LOGGER = Logger.getLogger(CrmStore.class);
    private final Properties connectionProperties = new Properties();
    private final String jdbcUrl;
    private final String schema;
    private Connection connection;

    public CrmStore(Properties configuration) {
        jdbcUrl = required(configuration, "crm.jdbcUrl");
        if (!jdbcUrl.startsWith("jdbc:postgresql:")) {
            throw new IllegalArgumentException("crm.jdbcUrl must use PostgreSQL");
        }
        schema = configuration.getProperty("crm.schema", "public");
        if (!schema.matches("[A-Za-z_][A-Za-z0-9_]*")) {
            throw new IllegalArgumentException("Invalid crm.schema");
        }
        connectionProperties.setProperty("user", required(configuration, "crm.username"));
        String environment = configuration.getProperty("crm.passwordEnv", "");
        String password =
                environment.trim().isEmpty()
                        ? configuration.getProperty("crm.password", "")
                        : System.getenv(environment);
        if (password == null) {
            throw new IllegalArgumentException("CRM password environment variable is not set");
        }
        connectionProperties.setProperty("password", password);
        connectionProperties.setProperty("connectTimeout", "5");
        connectionProperties.setProperty("socketTimeout", "30");
        connectionProperties.setProperty("ApplicationName", "SalesTrading-CRMSvr");
    }

    public CrmDataset load() throws SQLException {
        long started = System.nanoTime();
        connection = DriverManager.getConnection(jdbcUrl, connectionProperties);
        connection.setReadOnly(true);
        List<CrmDataset.Institution> institutions = institutions();
        List<CrmDataset.Contact> contacts = contacts();
        List<CrmDataset.ContactOrg> contactOrgs = contactOrgs();
        LOGGER.info(
                "CRM snapshot loaded: institutions="
                        + institutions.size()
                        + ", contacts="
                        + contacts.size()
                        + ", contactOrgs="
                        + contactOrgs.size()
                        + ", elapsedMillis="
                        + (System.nanoTime() - started) / 1000000);
        return new CrmDataset(institutions, contacts, contactOrgs);
    }

    private List<CrmDataset.Institution> institutions() throws SQLException {
        String sql =
                "SELECT id, org_name, org_short_name, org_name_py, org_short_name_py, org_type,"
                        + " cfets_6_code FROM "
                        + table("crm_org")
                        + " WHERE deleted = false";
        List<CrmDataset.Institution> rows = new ArrayList<>();
        try (PreparedStatement statement = connection.prepareStatement(sql)) {
            statement.setQueryTimeout(30);
            try (ResultSet result = statement.executeQuery()) {
                while (result.next()) {
                    rows.add(
                            new CrmDataset.Institution(
                                    result.getString("id"),
                                    result.getString("org_name"),
                                    result.getString("org_short_name"),
                                    result.getString("org_name_py"),
                                    result.getString("org_short_name_py"),
                                    result.getString("org_type"),
                                    result.getString("cfets_6_code")));
                }
            }
        }
        return rows;
    }

    private List<CrmDataset.Contact> contacts() throws SQLException {
        String sql =
                "SELECT id, name, position_name, qq, cfets_trader_id, cfets_trader_name FROM "
                        + table("crm_contact")
                        + " WHERE deleted = false";
        List<CrmDataset.Contact> rows = new ArrayList<>();
        try (PreparedStatement statement = connection.prepareStatement(sql)) {
            statement.setQueryTimeout(30);
            try (ResultSet result = statement.executeQuery()) {
                while (result.next()) {
                    rows.add(
                            new CrmDataset.Contact(
                                    result.getString("id"),
                                    result.getString("name"),
                                    result.getString("position_name"),
                                    result.getString("qq"),
                                    result.getString("cfets_trader_id"),
                                    result.getString("cfets_trader_name")));
                }
            }
        }
        return rows;
    }

    private List<CrmDataset.ContactOrg> contactOrgs() throws SQLException {
        String sql =
                "SELECT r.contact_id, r.org_id FROM "
                        + table("crm_contact_org_rel")
                        + " r JOIN "
                        + table("crm_org")
                        + " o ON o.id = r.org_id AND o.deleted = false"
                        + " WHERE r.deleted = false AND r.is_current = true";
        List<CrmDataset.ContactOrg> rows = new ArrayList<>();
        try (PreparedStatement statement = connection.prepareStatement(sql)) {
            statement.setQueryTimeout(30);
            try (ResultSet result = statement.executeQuery()) {
                while (result.next()) {
                    rows.add(
                            new CrmDataset.ContactOrg(
                                    result.getString("contact_id"), result.getString("org_id")));
                }
            }
        }
        return rows;
    }

    private String table(String name) {
        return "\"" + schema + "\".\"" + name + "\"";
    }

    private static String required(Properties configuration, String key) {
        String value = configuration.getProperty(key, "").trim();
        if (value.isEmpty()) {
            throw new IllegalArgumentException(key + " is required");
        }
        return value;
    }

    @Override
    public void close() {
        if (connection == null) {
            return;
        }
        try {
            connection.close();
        } catch (SQLException error) {
            LOGGER.warn("CRM connection close failed: reason=" + error.getMessage());
        } finally {
            connection = null;
        }
    }
}
