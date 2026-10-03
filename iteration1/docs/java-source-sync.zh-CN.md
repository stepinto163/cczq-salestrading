# Java 全量源码打包

Windows 安装 Python 3.9+ 和 Git，并加入 PATH。在任意目录执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "D:\workspace\greatewall\ficc\ficc\iteration1\scripts\pack-java-sources.ps1"
```

默认打包 `iteration1` 下全部 `cgws-ficc*` Maven 项目，ZIP 保存到 `iteration1/runtime/source-sync`，执行结束显示完整路径。

指定项目父目录和输出位置：

```powershell
.\pack-java-sources.ps1 -ProjectRoot "D:\workspace\GitHub" -Output "D:\temp\cgws-ficc-java-sources.zip"
```

脚本 `pack-java-sources.ps1` 与 `pack_java_sources.py` 必须放在同一目录。

包含已提交、修改未提交、新增未提交的完整源码，以及 POM、Jenkinsfile、control、salt-formula、build 定义、启动脚本和文档。保留项目名称作为 ZIP 顶层目录。

遵循 `.gitignore`，额外排除 target 系列目录、运行数据、日志、JAR/CLASS 等产物和本机凭据。不会删除源目录的任何构建结果。包内 `__source_sync__/manifest.json` 提供文件数量、SHA-256 和额外排除原因。

解压到新目录检查后同步。直接覆盖旧目录不会删除旧版本已经移除的文件；接收端的本机配置和 Git 历史需保留。依赖由接收端 Maven 环境提供，本包不是二进制部署包。
