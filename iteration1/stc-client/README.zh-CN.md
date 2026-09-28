# STC 销售交易客户端

源码入口：[stc-client](stc-client)。已作为普通源码目录纳入本仓库，不是需要访问内网的 Git submodule。

## 来源与分支

导入 `feature/ficc-lsj-integration` 的 `54d1a9186aef57f0410633b37a97f8135d682eda`。其父提交为 `feature/ficc-native-salestrading` 的 `336ba1b2c`，在我方 DPS 接入基础上合并同事 `dev_sales_trading_v0` 的产品界面。

本次从回收站恢复了原 `iteration1/lsj/stc-client`，确认 Git 祖先关系和合并说明后迁入当前路径；原始两个仓库的 Git 元数据保留在本机忽略目录，不上传完整内网历史。今后的客户端开发与服务端一起在本仓库分支维护。

## 构建

安装 Visual Studio Build Tools（MSBuild、WPF、.NET Framework 4.7.2 开发工具）。在 `stc-client/client/bin` 准备匹配的 STC QA 运行依赖，在 `client/control` 准备部署所需配置及公钥；二进制、许可证、用户数据和运行日志不随本次源码导入提交。不能仅复制一个插件 DLL 作为完整客户端。

从本仓库根目录运行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/build-native-client.ps1
```

输出位于 `stc-client/runtime/native-client`，与现有运行安装隔离。QA 配置及打包入口见[QA 客户端说明](stc-client/control/QA/README.zh-CN.md)；原生接入与尚未完成的功能见[合并说明](stc-client/docs/lsj-integration.zh-CN.md)。

旧设计笔记记录分阶段实施过程，当前默认业务实现为 `DpsSalesTradingServices`，复用宿主登录后的连接。Mock 实现仅供隔离测试，不代表服务已接通。

精确报价已接入现有 ParserSvr 协议，流程、字段映射、测试与真实服务验证边界见[自然语言报价接入](stc-client/docs/parser-integration.zh-CN.md)。

## UI 端到端测试

[测试管理窗口](stc-client/docs/ui-automation.zh-CN.md)支持新增、编辑、复制、删除用例，按步骤配置参数，运行单例/套件并实时查看结果、截图和日志。使用真实 STC 服务及数据库，每例独立数据并自动清理。

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/start-ui-tests.ps1 -Build
```

## 本机客户端管理

在本机 `qa_env\ats_root\scripts` 中运行 `client.cmd start|stop|restart|status`。客户端独立于后端“全部启动/停止”，默认使用 `qa_env\stc-client\STC.exe.config`，运行上述最新构建输出。

版本管理中的脚本为 [manage-stc-client.ps1](../scripts/manage-stc-client.ps1)，也可从仓库根目录直接调用：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/manage-stc-client.ps1 start
```

可用 `-StcConfig <配置路径>` 切换环境。脚本只管理完整路径匹配的客户端，正常关闭超时会报错；强制关闭必须显式指定 `-Force`。脚本不自动编译或启动后端，`status` 不代表登录及服务可用性检查。
