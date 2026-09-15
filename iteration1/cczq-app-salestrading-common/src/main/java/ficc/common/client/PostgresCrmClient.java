package ficc.common.client;

import com.alibaba.fastjson2.JSONArray;
import com.alibaba.fastjson2.JSONObject;
import ficc.common.ApiException;
import ficc.common.JsonUtils;
import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import org.apache.log4j.Logger;
import sc.app.stc.net.NetClientWrapper;

/** Read-only CRM integration using a connection independent of the trading database. */
public final class PostgresCrmClient implements CrmClient {
    private static final Logger LOGGER = Logger.getLogger(PostgresCrmClient.class);
    private final Properties connectionProperties = new Properties();
    private String jdbcUrl;
    private String schema;

    @Override
    public void initialize(NetClientWrapper network, Properties configuration) {
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
        String password = environment.isBlank() ? configuration.getProperty("crm.password", "") : System.getenv(environment);
        if (password == null) {
            throw new IllegalArgumentException("CRM password environment variable is not set");
        }
        connectionProperties.setProperty("password", password);
        connectionProperties.setProperty("connectTimeout", "3");
        connectionProperties.setProperty("socketTimeout", "8");
        connectionProperties.setProperty("ApplicationName", "SalesTrading-CRM");
        execute("INSTITUTION", "", null, null, 0, 1);
        execute("CONTACT", "", null, null, 0, 1);
    }

    private static String required(Properties configuration, String key) {
        String value = configuration.getProperty(key, "").trim();
        if (value.isEmpty()) {
            throw new IllegalArgumentException(key + " is required");
        }
        return value;
    }

    @Override
    public JSONObject query(String userId, String kind, String text, String institutionId, int offset, int limit) {
        return execute(kind, text, institutionId, null, offset, limit);
    }

    @Override
    public List<JSONObject> institutions(String userId) {
        throw new UnsupportedOperationException("Use bounded CRM query or lookup by institutionId");
    }

    @Override
    public List<JSONObject> contacts(String userId) {
        throw new UnsupportedOperationException("Use bounded CRM query or lookup by contactId");
    }

    @Override
    public JSONObject institution(String userId, String institutionId) {
        return find("INSTITUTION", institutionId);
    }

    @Override
    public JSONObject contact(String userId, String contactId) {
        return find("CONTACT", contactId);
    }

    private JSONObject find(String kind, String id) {
        JSONArray records = execute(kind, "", null, id, 0, 1).getJSONArray("records");
        return records.isEmpty() ? null : records.getJSONObject(0);
    }

    private String table(String name) {
        return "\"" + schema + "\".\"" + name + "\"";
    }

    private String directory(String kind) {
        if ("INSTITUTION".equals(kind)) {
            return "SELECT o.id AS sort_id, o.id::text AS id, o.org_name AS name, o.org_short_name AS short_name, "
                    + "concat_ws(' ', o.id::text, o.org_name, o.org_short_name, o.org_name_py, o.org_short_name_py) AS search_text "
                    + "FROM " + table("crm_org") + " o WHERE o.deleted = false";
        }
        if (!"CONTACT".equals(kind)) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "Invalid CRM kind", "kind");
        }
        return "SELECT c.id AS sort_id, c.id::text AS id, c.name, c.position_name, "
                + "concat_ws(' ', c.id::text, c.name, c.cfets_trader_id, c.cfets_trader_name) AS search_text, "
                + "COALESCE((SELECT json_agg(DISTINCT r.org_id::text) FROM " + table("crm_contact_org_rel") + " r "
                + "JOIN " + table("crm_org") + " o ON o.id = r.org_id AND o.deleted = false "
                + "WHERE r.contact_id = c.id AND r.deleted = false AND r.is_current = true), '[]'::json) AS institution_ids "
                + "FROM " + table("crm_contact") + " c WHERE c.deleted = false";
    }

    private JSONObject execute(String kind, String text, String institutionId, String id, int offset, int limit) {
        if (offset < 0 || offset > 1000000 || limit < 1 || limit > 200) {
            throw ApiException.forField("1001", "VALIDATION_FAILED", "CRM offset/limit out of range", "limit");
        }
        long started = System.nanoTime();
        StringBuilder filter = new StringBuilder(" WHERE true");
        List<Object> parameters = new ArrayList<>();
        if (id != null) {
            filter.append(" AND id = ?");
            parameters.add(id);
        }
        if (text != null && !text.isBlank()) {
            filter.append(" AND search_text ILIKE ?");
            parameters.add("%" + text.trim().replace("\\", "\\\\").replace("%", "\\%").replace("_", "\\_") + "%");
        }
        if ("CONTACT".equals(kind) && institutionId != null && !institutionId.isBlank()) {
            filter.append(" AND id IN (SELECT r.contact_id FROM " + table("crm_contact_org_rel") + " r JOIN "
                    + table("crm_org") + " o ON o.id = r.org_id AND o.deleted = false "
                    + "WHERE r.org_id = ? AND r.deleted = false AND r.is_current = true)");
            parameters.add(institutionId);
        }
        String sql = "SELECT * FROM (" + directory(kind) + ") directory" + filter + " ORDER BY sort_id LIMIT ? OFFSET ?";
        parameters.add(limit + 1);
        parameters.add(offset);
        LOGGER.debug("CRM query: kind=" + kind + ", parameters=" + JsonUtils.toJson(parameters));
        try (Connection connection = DriverManager.getConnection(jdbcUrl, connectionProperties)) {
            connection.setReadOnly(true);
            try (PreparedStatement statement = connection.prepareStatement(sql)) {
                statement.setQueryTimeout(5);
                for (int index = 0; index < parameters.size(); index++) {
                    statement.setObject(index + 1, parameters.get(index));
                }
                JSONArray records = new JSONArray();
                try (ResultSet result = statement.executeQuery()) {
                    while (result.next()) {
                        records.add(map(kind, result));
                    }
                }
                boolean hasMore = records.size() > limit;
                if (hasMore) {
                    records.remove(records.size() - 1);
                }
                LOGGER.info("CRM query completed: kind=" + kind + ", rows=" + records.size() + ", hasMore=" + hasMore
                        + ", elapsedMillis=" + (System.nanoTime() - started) / 1000000);
                return JsonUtils.object("records", records, "hasMore", hasMore, "nextOffset", hasMore ? offset + limit : null);
            }
        } catch (SQLException error) {
            LOGGER.warn("CRM query failed: kind=" + kind + ", sqlState=" + error.getSQLState()
                    + ", errorCode=" + error.getErrorCode() + ", reason=" + error.getMessage()
                    + ", elapsedMillis=" + (System.nanoTime() - started) / 1000000);
            ApiException failure = ApiException.forField("1009", "NOT_READY",
                    "CRM database unavailable; check CRM configuration and server log", null);
            failure.initCause(error);
            throw failure;
        }
    }

    private JSONObject map(String kind, ResultSet result) throws SQLException {
        if ("INSTITUTION".equals(kind)) {
            return JsonUtils.object("institutionId", result.getString("id"), "institutionName", result.getString("name"),
                    "institutionShortName", result.getString("short_name"));
        }
        JSONArray institutions = JSONArray.parseArray(result.getString("institution_ids"));
        return JsonUtils.object("contactId", result.getString("id"), "contactName", result.getString("name"),
                "positionName", result.getString("position_name"), "institutionIds", institutions,
                "institutionId", institutions.size() == 1 ? institutions.getString(0) : null);
    }
}
