# 本机 QA Maven 依赖准备

业务模块只维护主 `pom.xml`，使用 Java 8 和 QA 原版 JAR，不重编译 STC 基础库或 GW。

`mvn-root.pom.xml` 是经用户授权重建的最小本机父 POM，坐标为 `sumscope:mvn-root:1.0.0`。
它不是 SumScope 原始文件，仅补充编码与 Java 8 默认配置；不包含 QA 的发布地址、凭据或未知构建逻辑。
只允许本机 install，不能覆盖内网仓库的同名构件。取得原始 POM 后，应在本机重新 install 原始文件。

导入脚本保留 `sc-app-stc`、`sc-app-common` JAR 内的原始 POM；其他私有 JAR 的坐标取自这两个 POM，
通过 Maven install-file 注册，不修改 JAR。没有原始 POM 的私有依赖使用生成的基础元数据，不能据此声称与 QA 依赖图完全一致。
每次导入在 `runtime/qa-maven-bootstrap/manifest.json` 记录来源、坐标和 SHA-256。

```powershell
python iteration1/scripts/bootstrap-qa-maven.py `
  --qa-lib "D:\workspace\greatewall\ficc\qa_env\ats_root\stc\BaseSvr\javalib" `
  --stc-parent-pom "D:\workspace\GitHub\sc-app-stc-parent-pom\pom.xml" `
  --reconstruct-root `
  --maven "D:\workspace\greatewall\ficc\ficc\iteration1\runtime\tools\apache-maven-3.9.16\bin\mvn.cmd"
```

已有原始根 POM 时用 `--root-pom <路径>` 代替 `--reconstruct-root`。脚本不会自动替换缓存中已有的根 POM。
公开依赖由 Maven Central 解析。`settings-local.xml` 仅供本机使用，屏蔽旧依赖 POM 中不可访问的内网仓库；Jenkins 继续使用自己的 settings。

```powershell
powershell -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1 `
  -SettingsFile iteration1/build/qa-maven/settings-local.xml
python iteration1/scripts/verify-maven-packages.py
```

2026-09-28 已完成：主 POM 的 parent/common 安装、四个服务编译打包；不再依赖旧兼容坐标。
