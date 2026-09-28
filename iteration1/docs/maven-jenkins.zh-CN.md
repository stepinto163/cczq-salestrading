# Maven / Jenkins 构建

本机与 QA 统一使用 Java 8、Maven 3.9.x 和每个项目的主 `pom.xml`。不再维护第二套兼容坐标、补丁基础库或 GW 编译步骤。

构建顺序：`cgws-ficc-saletrading-parent-pom` → `common` → `quotesvr`、`notificationsvr`、`crmsvr`、`qtintakesvr`。
父 POM 继承 `sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT`。Jenkins 继续使用自己的 Maven settings、共享流水线和 QA salt-formula；跨节点先发布 parent/common，再构建服务。

本机首次准备 QA 依赖及缺失父 POM，见 [QA Maven 依赖准备](../build/qa-maven/README.zh-CN.md)。准备好后：

```powershell
powershell -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1 `
  -SettingsFile iteration1/build/qa-maven/settings-local.xml
python iteration1/scripts/verify-maven-packages.py
```

服务产物为 `target/<artifact>-0.1.0-SNAPSHOT.jar`、`target/javalib/`、`target/bin/start`。
QA 部署包由 Jenkins 将这些产物与 salt-formula 配置组装，见 [QA Jenkins](jenkins-qa.zh-CN.md)。
GW 使用已有 QA 可运行包。地址、数据库、许可证等环境配置不随依赖导入覆盖。

2026-09-28 验证：主 POM 构建 parent/common 与四个服务成功，95 项测试通过、2 项数据库集成测试按条件跳过；四个服务的入口、依赖文件及业务 Java 8 字节码检查通过。
重建根 POM 不等于取得 QA 原始父 POM，原文件仍应补回。
