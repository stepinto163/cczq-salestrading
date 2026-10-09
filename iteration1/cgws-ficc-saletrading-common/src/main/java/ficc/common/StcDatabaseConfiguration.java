package ficc.common;

import ats.foundation.utils.config.PropertiesHelper;
import ats.foundation.utils.db.base.DBPoolParam;
import ats.foundation.utils.security.PasswordSecurityHandler;

import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;
import java.util.function.Consumer;

/**
 * 从共享 DBPoolConfig.ini 按数据源名称读取连接信息，复用 STC 的 INI/include 解析与解密方式。
 * 配置 db.sourceName/crm.sourceName 后，对应连接地址、用户名和密码以共享数据源为准。
 * 这里只读取并覆盖当前服务的 Properties，不修改共享文件，也不改变其他 STC 进程的数据源选择。
 */
final class StcDatabaseConfiguration {
    private StcDatabaseConfiguration() {}

    static void apply(Properties configuration, Consumer<String> log) throws IOException {
        String databaseSource = configuration.getProperty("db.sourceName", "").trim();
        String crmSource = configuration.getProperty("crm.sourceName", "").trim();
        if (databaseSource.isEmpty() && crmSource.isEmpty()) {
            return;
        }
        Path path =
                ServiceConfiguration.resolvePath(
                        configuration.getProperty(
                                "dbpool.cfg", "$ATS_ROOT/control/DBPoolConfig.ini"),
                        System.getenv("ATS_ROOT"));
        List<DBPoolParam> sources;
        try {
            sources = readSources(path);
        } catch (Exception error) {
            throw new IOException("Cannot load STC database configuration: " + path, error);
        }
        if (!databaseSource.isEmpty()) {
            applySource(configuration, sources, databaseSource, "db.url", "db.user", "db.password");
        }
        if (!crmSource.isEmpty()) {
            applySource(
                    configuration,
                    sources,
                    crmSource,
                    "crm.jdbcUrl",
                    "crm.username",
                    "crm.password");
            configuration.remove("crm.passwordEnv");
        }
        log.accept(
                "STC database configuration: file="
                        + path
                        + ", db.sourceName="
                        + databaseSource
                        + ", crm.sourceName="
                        + crmSource);
    }

    // Reuse STC's INI/include handling, but own and close the input stream.
    // DBUtils.readConfig(String) in the supplied library leaves its file stream open.
    static List<DBPoolParam> readSources(Path path) throws Exception {
        PropertiesHelper properties = new PropertiesHelper();
        try (InputStream input = Files.newInputStream(path)) {
            properties.load(input);
        }
        int count = Integer.parseInt(properties.getProperty("DBPOOL.DBCount", "0"));
        if (count <= 0) {
            throw new IOException("DBPOOL.DBCount must be positive");
        }
        List<DBPoolParam> sources = new ArrayList<>();
        for (int index = 0; index < count; index++) {
            DBPoolParam source = new DBPoolParam();
            source.SourceName = properties.getProperty("DBPOOL.DBSourceName_" + index);
            source.Driver = properties.getProperty("DBPOOL.DBDriver_" + index);
            source.Url = properties.getProperty("DBPOOL.DBUrl_" + index);
            source.Username = properties.getProperty("DBPOOL.DBUsername_" + index);
            source.Passwd = properties.getProperty("DBPOOL.DBPasswd_" + index);
            source.IsEncrypt =
                    Boolean.parseBoolean(
                            properties.getProperty("DBPOOL.DBIsEncrypt_" + index, "false"));
            sources.add(source);
        }
        return sources;
    }

    private static void applySource(
            Properties configuration,
            List<DBPoolParam> sources,
            String sourceName,
            String urlKey,
            String userKey,
            String passwordKey)
            throws IOException {
        DBPoolParam selected = null;
        for (DBPoolParam source : sources) {
            if (sourceName.equals(source.SourceName)) {
                if (selected != null) {
                    throw new IOException("Duplicate STC database source: " + sourceName);
                }
                selected = source;
            }
        }
        if (selected == null) {
            throw new IOException("Missing or invalid STC database source: " + sourceName);
        }
        if (selected.Url == null
                || !selected.Url.startsWith("jdbc:postgresql:")
                || selected.Username == null
                || selected.Username.trim().isEmpty()
                || selected.Passwd == null) {
            throw new IOException("Incomplete PostgreSQL database source: " + sourceName);
        }
        configuration.setProperty(urlKey, selected.Url);
        configuration.setProperty(userKey, selected.Username);
        String password = selected.Passwd;
        if (selected.IsEncrypt) {
            try {
                PasswordSecurityHandler handler = PasswordSecurityHandler.getHandler();
                if (handler == null) {
                    throw new IOException(
                            "STC password decryption is unavailable for " + sourceName);
                }
                password = handler.decrypt(password, true);
            } catch (Exception error) {
                throw new IOException("STC password decryption failed for " + sourceName, error);
            }
        }
        configuration.setProperty(passwordKey, password);
    }
}
