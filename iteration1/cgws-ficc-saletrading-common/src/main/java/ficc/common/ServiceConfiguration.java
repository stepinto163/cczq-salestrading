package ficc.common;

import ficc.common.client.StcBaseDataClient;

import java.io.BufferedReader;
import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.security.CodeSource;
import java.util.Arrays;
import java.util.List;
import java.util.Properties;
import java.util.function.Consumer;
import java.util.stream.Collectors;

/**
 * 服务配置入口：先读取业务配置，再读取适配器配置，最后解析共享 DBPool 数据源。
 * 同名属性以后读入的值为准；数据库字段的覆盖规则由 StcDatabaseConfiguration 负责。
 * 相对路径以进程工作目录为基准，$ATS_ROOT/ 前缀指向共享部署目录。
 * 文件缺失或解析失败会向上传播，不能把“配置加载成功”等同于远端服务已就绪。
 */
public final class ServiceConfiguration {
    private ServiceConfiguration() {}

    public static Properties load(String serviceName, String databaseFile, String adapterFile)
            throws IOException {
        Properties context = new Properties();
        context.setProperty("diagnostics.serviceName", serviceName);
        String files =
                "workingDirectory="
                        + Paths.get("").toAbsolutePath().normalize()
                        + ", quote.properties="
                        + databaseFile
                        + ", adapter.properties="
                        + adapterFile;
        try {
            return ComponentLog.initialize(
                    context,
                    "CONFIGURATION",
                    "files",
                    "ServiceConfiguration",
                    files,
                    () ->
                            load(
                                    serviceName,
                                    databaseFile,
                                    adapterFile,
                                    org.apache.log4j.Logger.getLogger("ficc.startup")::info));
        } catch (IOException error) {
            throw error;
        } catch (Exception error) {
            throw new IOException("Cannot load service configuration", error);
        }
    }

    static Properties load(
            String serviceName, String databaseFile, String adapterFile, Consumer<String> log)
            throws IOException {
        Properties configuration = new Properties();
        Path databasePath = resolvePath(databaseFile, System.getenv("ATS_ROOT"));
        loadFile(configuration, databasePath);
        Path adapterPath = null;
        if (adapterFile != null && !adapterFile.trim().isEmpty()) {
            adapterPath = resolvePath(adapterFile, System.getenv("ATS_ROOT"));
            loadFile(configuration, adapterPath);
        }
        StcDatabaseConfiguration.apply(configuration, log);
        log.accept(
                serviceName
                        + " configuration: workingDirectory="
                        + Paths.get("").toAbsolutePath().normalize());
        log.accept(
                serviceName
                        + " configuration: quote.properties="
                        + databasePath
                        + ", adapter.properties="
                        + (adapterPath == null ? "<not configured>" : adapterPath));
        List<String> securityIds =
                Arrays.stream(configuration.getProperty("base.securityIds", "").split(","))
                        .map(String::trim)
                        .filter(value -> !value.isEmpty())
                        .distinct()
                        .collect(Collectors.toList());
        String baseMode = configuration.getProperty("base.mode", "simulator");
        String loadMode = "stc".equals(baseMode) ? "stc-client-on-demand" : "simulator";
        // Do not dump properties: the same files can contain database and service credentials.
        log.accept(
                serviceName
                        + " adapters: identity.mode="
                        + configuration.getProperty("identity.mode", "simulator")
                        + ", base.mode="
                        + baseMode
                        + ", base.loadMode="
                        + loadMode
                        + ", crm.mode="
                        + configuration.getProperty("crm.mode", "simulator")
                        + ", securityIdsCount="
                        + securityIds.size()
                        + ", securityIdsSample="
                        + securityIds.stream().limit(5).collect(Collectors.toList()));
        CodeSource source = StcBaseDataClient.class.getProtectionDomain().getCodeSource();
        log.accept(
                serviceName
                        + " base adapter: diagnostics=stc-base-config-v2, loadedFrom="
                        + (source == null ? "<unavailable>" : source.getLocation()));
        configuration.setProperty("diagnostics.serviceName", serviceName);
        return configuration;
    }

    static Path resolvePath(String configuredPath, String atsRoot) throws IOException {
        if (configuredPath == null || configuredPath.trim().isEmpty()) {
            throw new IOException("Service configuration path is empty");
        }
        String rootPrefix = "$ATS_ROOT/";
        if (configuredPath.startsWith(rootPrefix)) {
            if (atsRoot == null || atsRoot.trim().isEmpty()) {
                throw new IOException("ATS_ROOT is required for " + configuredPath);
            }
            return Paths.get(atsRoot)
                    .resolve(configuredPath.substring(rootPrefix.length()))
                    .toAbsolutePath()
                    .normalize();
        }
        return Paths.get(configuredPath).toAbsolutePath().normalize();
    }

    private static void loadFile(Properties configuration, Path path) throws IOException {
        try (BufferedReader reader = Files.newBufferedReader(path)) {
            configuration.load(reader);
        }
    }
}
