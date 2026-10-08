# FICC 源码可控框架

[约束、设计、实测结果和未完成范围](../docs/framework-replacement.zh-CN.md)。分支 `feature/ficc-lightweight-runtime` 已让五个 FICC 服务使用本框架，并完成 Windows 隔离进程协议、WPF 桌面用例及主备接管验证；尚未覆盖日常环境或 QA。

## 代码导航

- `core/.../LifecycleActions.java`：统一组件日志与失败传播。
- `compat/.../ats/bbox/framework/`：原 XML/INI 配置组合、组件构造、资源与流生命周期、失败清理。
- `dps/.../DpsServer.java` / `DpsPeer.java`：原 DPS SDK 的服务端/客户端适配；不实现线协议。
- `dps/.../ZookeeperDiscovery.java`：原 STC 发现契约，注册就绪状态及会话恢复。
- `compat/.../sc/app/stc/net/`：服务源码使用的原网络 API。
- `compat/.../sc/app/stc/utils/ServerWrapper.java`：保留服务入口调用方式。
- `recovered-support.json`：恢复的客户端、DTO、配置/数据库工具源码与二进制来源。
- `compat/src/test/.../DeployedBaselineTest.java`：对部署 BBox 二进制进行独立类加载器对照。

## 构建与验证

需要 JDK 8、Python 3、本机 Maven 缓存及现有 QA SDK。普通构建在仓库根目录执行 `build quotesvr` 或 `build parsersvr`；脚本先构建框架，再构建业务工程。`deploy` 会替换并重启目标进程；本次验收没有执行它。

仅构建及功能检查，不启动业务服务：

```powershell
python iteration1/scripts/test-framework.py --legacy-jars ../qa_env/ats_root/stc/QuoteSvr/javalib --license ../qa_env/ats_root/control/server.lic --services
```

此命令 install 框架/父 POM/common 到本机 Maven，五个服务使用 `target/framework-check`。Maven 堆 256 MB，测试堆 128 MB，逐进程 watchdog。校验旧框架 JAR 已移除、DPS 二进制未变以及业务源码/配置哈希；不发布远程 Maven。

在已有本机 PostgreSQL/ZooKeeper/BaseSvr 上运行隔离进程验证：

```powershell
# 先为当前终端设置 FICC_TEST_PASSWORD；值不写入脚本或仓库。
python iteration1/scripts/test-framework-live.py --ats-root ../qa_env/ats_root --user test2
```

该脚本启动测试专用的 Quote/Notification/CRM/Parser/QtIntake 和标准 LoginSvr/GW。它复制环境配置，使用独立注册名、端口及数据库 schema；FIX 仅连接本机 acceptor。测试结束只关闭自身进程。独立 schema 和日志保留供复核，原有运行服务不替换。报告在 `iteration1/runtime/framework-live/`。

## 兼容边界

当前配置范围为五个 FICC 服务的 DPS + ZK active/standby 模式。保留原包名/API 和配置布局，不等于全部历史 STC 功能已经完成。DB cluster、monitor/encryption/管理 GUI、Linux/QA 部署仍未完成。standby 是冷备，失租约后原主机停止，重启可重新参与，不保证原桌面会话无感恢复。ParserSvr Java 主引擎已验通；native shadow 的本机数据库缺表单独记录，不伪装为通过。



## 桌面与主备验证

先生成独立的桌面测试宿主，再运行现有 17 例及主备流程：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/build-ui-tests.ps1 -OutputRoot "$PWD/iteration1/runtime/framework-ui-client"
# 当前终端设置 FICC_TEST_PASSWORD 后执行：
python iteration1/scripts/test-framework-live.py --ats-root ../qa_env/ats_root --user test2 --ui-cases all --standby-test
```

`--ui-cases` 也接受逗号分隔的现有用例 ID。`--standby-test` 启动两个冷备、验证控制脚本识别等待状态，强杀本次启动的 QuoteSvr/NotificationSvr 主实例，等待原 ZooKeeper 会话超时，通过未重启的 GW 再次验证。指定 UI 时，接管后再运行两项桌面用例。它不会杀掉原日常进程，不连接真实 QTrade。

这不是负载测试。每例独立 UI worker，有独立超时与清理；套件有总 watchdog。报告及截图在 `framework-live/<时间>/ui-reports`、`ui-after-takeover`。本机进程控制源码位于 `iteration1/scripts/manage-services.ps1`，等待状态检查可用 `test-service-standby-state.ps1` 单独验证。
