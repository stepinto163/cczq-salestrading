# Maven / Jenkins 构建

后端源码以 iteration1/cgws-ficc-saletrading-* 为准，旧 cczq-app-* 项目已从当前版本删除，历史实现可通过 Git 历史查看。

## 正式入口

使用 **JDK 8、Maven 3.9.x**。四个项目分别构建，不使用根聚合 POM：

1. cgws-ficc-saletrading-parent-pom
2. cgws-ficc-saletrading-common
3. cgws-ficc-saletrading-quotesvr
4. cgws-ficc-saletrading-notificationsvr

每个项目的 **pom.xml 是 STC 主入口**。父 POM 继承 sumscope.stc:sc-app-stc-parent-pom:0.1.1-SNAPSHOT；业务模块使用 cgws.ficc.saletrading 坐标。parent/common 使用 sslib@0.0.1 和 building.buildJavaArtifact；两个业务服务使用 QA 的 sslib@main 和 building.buildJavaService。内网依赖及 settings.xml 由 Jenkins 环境提供。

在同一机器按上述顺序运行 `mvn -B -ntp clean install`。跨 Jenkins 节点时，先将 parent-pom、common 执行 deploy 发布到内网仓库，再构建服务。标准服务构建输出 target/bin、target/javalib 和业务 JAR；Jenkins 用 salt-formula 生成 QA.APP 的 tar.gz 包。rel_ 应用分支部署到 10.130.21.53 的 /opt/sumscope/stc，使用 sumscope 账号及既有 start/stop 脚本，详见 [QA Jenkins](jenkins-qa.zh-CN.md)。标准 GW、C#、Web 不属于 Java 构建。

## 本机兼容入口

明确使用 pom_compat.xml，坐标为 cgws.ficc.saletrading.compat，输出 target-compat；不能把它作为内网 STC 默认入口。

```powershell
powershell -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1
python iteration1/scripts/verify-maven-packages.py
```

兼容依赖需要先安装到本地 Maven 仓库，具体准备方法见 [父项目说明](../cgws-ficc-saletrading-parent-pom/README.md)。本机启动脚本从 Maven 部署包提取与 manifest 对应的依赖，不再使用旧手工 javac 产物。

## 配置与验证

服务工作目录设为当前服务目录。本机兼容启动继续使用 config/database.properties；QA 使用共享 $ATS_ROOT/control/DBPoolConfig.ini，服务包内只保留数据源选择及适配器设置。密码和许可证不提交。

本轮已经实际使用 JDK 8 完成兼容构建、31 项单元测试和 50 项有界 PostgreSQL 功能检查；1 项 CRM 集成测试按条件跳过。正式内网依赖和 Jenkins 共享流水线仍需在 STC 环境验证。
