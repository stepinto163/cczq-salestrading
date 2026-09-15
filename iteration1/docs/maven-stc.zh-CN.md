# STC Maven 构建

同一份源码、两套构建入口，不需要长期维护环境分支：

| 环境 | 根入口 | 依赖来源 | 输出 |
|---|---|---|---|
| 当前笔记本兼容环境 | `iteration1/pom.xml` | 本地已准备的兼容制品 | `target-maven` |
| WorkBuddy / 内网 STC | `iteration1/pom_stc.xml` | STC 父 POM 及内网 Maven 仓库 | `target-stc` |

原有 `pom.xml` 不改名；本轮新增五个 `pom_stc.xml`：根父 POM、Common、QuoteSvr、NotificationSvr、gateway。根 modules 显式指向各模块的 `pom_stc.xml`；子模块的 parent.relativePath 指向 `../pom_stc.xml`，避免误用旁边的兼容 POM。

## 在内网构建

从仓库根目录执行，先确保 Maven 使用 JDK 17：

```powershell
mvn -v
mvn -B -ntp -f iteration1/pom_stc.xml clean verify
```

如内网仓库配置不在默认 settings.xml：

```powershell
mvn -s E:\path\to\settings.xml -B -ntp -f iteration1/pom_stc.xml clean verify
```

上面的 settings 路径是示例，请换成实际文件。仓库地址、mirror、凭据和 snapshot 策略由该文件配置，不提交到项目。

Reactor 应包括根父 POM及4个模块，**GW始终参与构建**。Java编译和JUnit 5测试插件在父POM中显式启用，覆盖继承的Java 8编译设置。测试最多一个128MB JVM，30秒进程超时。构建不会启动服务或压力测试。

如果只构建 QuoteSvr 及其依赖：

```powershell
mvn -f iteration1/pom_stc.xml -pl :cczq-app-salestrading-quotesvr -am clean verify
```

对子模块单独执行 `mvn -f <模块>/pom_stc.xml ...` 也能解析正确父 POM，但模块间依赖需已安装；优先从根入口使用 `-pl` / `-am`。

## 依赖与坐标

- 根STC版父POM继承 `sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT`，通过 `<relativePath/>` 从Maven仓库解析，不依赖本机父源码目录。
- `sc-app-stc`、`sc-app-common`、ATS等沿用STC父POM提供的依赖，不引用 `cczq.salestrading.compat` 或 `stc-foundation-runtime`。
- Common保留用户已编译通过的 `mina-core:1.1.7` 及其日志依赖排除，只声明一次；JUnit 5用于测试，PostgreSQL驱动作为显式运行时依赖，随服务包携带。
- GW使用 `sumscope.stcl:gw:0.1.1-SNAPSHOT`，坐标来自提供的GW源码POM。注意这里是 `stcl`，不是 `stc`。是否已发布及其API是否与扩展代码匹配，需要在内网构建确认。实际发布坐标不同，可修改根POM的 `stc.gw.groupId` / `stc.gw.artifactId` / `stc.gw.version`，或通过同名 `-D` 参数覆盖；不要重新引入 `gw-local`。
- 项目自身坐标仍为 `cczq.salestrading:*:0.1.0-SNAPSHOT`，Common消费者通过父POM统一管理版本。
- GW的STC POM显式使用 `zookeeper` 和 `zookeeper-jute` 3.8.6，版本由模块属性 `stc.gw.zookeeper.version` 统一配置。上游GW源码声明的3.4.14在Java 17上有地址解析兼容问题；升级的是GW进程的客户端库，不是QA ZooKeeper服务端。排除该依赖引入的SLF4J API/Logback，沿用STC已有日志组件。

### GW 的 DPS 模式与 ZooKeeper 地址错误

`control/ATSConfig.ini` 使用 `NetType=gw`。源码 `CedaApi.initNodeClientWrapper()` 在 `ceda` 分支使用 `CedaClientWrapper`，其他分支使用 `GwClientWrapper`；因此 `CedaProxy/CedaApi` 类名不代表实际用了CEDA。DPS仍通过ZooKeeper发现服务地址，ZooKeeper异常与业务消息传输类型是不同层次的问题。

`SaslServerPrincipal` 报 `Unable to canonicalize address .../<unresolved>` 与ZooKeeper开发邮件中记录的3.4.14/Java 17问题一致：[开发邮件](https://www.mail-archive.com/dev@zookeeper.apache.org/msg75112.html)。本轮通过升级GW客户端依赖处理，不修改IP、不默认关闭SASL。3.8.6版本依据：[Apache发布说明](https://zookeeper.apache.org/doc/r3.8.6/releasenotes.html)。

拉取更新后，内网执行以下命令并重新加载IntelliJ Maven项目，再启动GW：

```powershell
mvn -B -ntp -f iteration1/pom_stc.xml -pl :cczq-app-salestrading-gw -am clean verify
mvn -f iteration1/gateway/pom_stc.xml dependency:tree "-Dincludes=org.apache.zookeeper:*"
```

依赖树中的 `zookeeper` 和 `zookeeper-jute` 应均为3.8.6。若仍有同样错误，在调试器求值 `org.apache.zookeeper.Version.getVersion()`，以及 `org.apache.zookeeper.ZooKeeper.class.getProtectionDomain().getCodeSource().getLocation()`，确认实际加载版本及JAR位置。

本机验证：POM XML及差异检查通过；Java 17.0.20.1 + 官方ZooKeeper 3.8.6客户端 + 现有STC使用的SLF4J 1.7.7/Log4j日志库，以默认SASL设置连接本机 `127.0.0.1:12181`，完成一次根节点只读查询并退出，退出码0，外部进程超时20秒，零写入。日志位于 `iteration1/runtime/zk-client-smoke.log`。未连接QA，也未以内部STC父POM完成本机整包构建，完整GW依赖组合仍需内网验证。

两套构建虽然文件名不同，产物坐标仍相同。在同一台机器上交替 `install` 会覆盖同一仓库坐标；`target-stc` 仅隔离构建目录。需要同机并存安装时，为STC版指定独立本地仓库：

```powershell
mvn -f iteration1/pom_stc.xml "-Dmaven.repo.local=E:/maven-repository-stc" clean install
```

不同机器可各用自己的默认本地仓库。不要把内容不同的兼容版和STC版当成同一共享仓库版本交替发布。

## 产物及运行

Common输出普通JAR；QuoteSvr、NotificationSvr、GW各输出业务JAR和 `*-dist.zip`。沿用 `build/service-assembly.xml`，包含control/config、运行库及SQL，排除许可证与本机私有properties。

部署使用各模块 `target-stc` 中的ZIP，按包内说明设置配置、许可证和ATS_ROOT后运行。原有 `start-*.ps1` 仍使用兼容版target/runtime库，**不会自动切换到STC构建产物**；不要据此声称已运行STC版。构建POM也不会自动把模拟依赖切成真实STC连接。

## 验证范围

本机检查新增POM的XML、模块入口、父链、依赖管理及输出路径，确认原有兼容POM未改。笔记本没有内网STC父POM，不能在此验证该父POM及已发布JAR的完整有效模型、编译和运行兼容性；最终以内网 `clean verify` 及启动检查为准。若继承的STC父POM含额外打包执行，也需要根据effective-pom核对，不能单凭模块源码编译通过认定整个部署包正确。

```powershell
# 排查实际继承配置时使用；结果留在本机。
mvn -f iteration1/cczq-app-salestrading-common/pom_stc.xml help:effective-pom -Doutput=target-stc/effective-pom.xml
```

本轮不恢复已取消的WorkBuddy自动任务，用户可以手工检出并构建。
