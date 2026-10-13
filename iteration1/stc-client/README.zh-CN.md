# STC 销售交易客户端

源码入口：[stc-client](stc-client)。已作为普通源码目录纳入本仓库，不是需要访问内网的 Git submodule。

## 来源与分支

导入 `feature/ficc-lsj-integration` 的 `54d1a9186aef57f0410633b37a97f8135d682eda`。其父提交为 `feature/ficc-native-salestrading` 的 `336ba1b2c`，在我方 DPS 接入基础上合并同事 `dev_sales_trading_v0` 的产品界面。

本次从回收站恢复了原 `iteration1/lsj/stc-client`，确认 Git 祖先关系和合并说明后迁入当前路径；原始两个仓库的 Git 元数据保留在本机忽略目录，不上传完整内网历史。今后的客户端开发与服务端一起在本仓库分支维护。

## 构建

安装 Visual Studio Build Tools（MSBuild、WPF、.NET Framework 4.7.2 开发工具）。在 `stc-client/client/bin` 准备匹配的 STC QA 运行依赖，在 `client/control` 准备部署所需配置及公钥；二进制、许可证、用户数据和运行日志不随本次源码导入提交。不能仅复制一个插件 DLL 作为完整客户端。

Visual Studio 中打开 `stc-client/STC.sln`，还原 NuGet 包后生成解决方案。`common.netapi` 通过项目引用从源码先行构建，所有平台配置均输出到 `client/bin`，插件复用宿主目录中的程序集。RocksDB 和 Ats.Foundation 引用同一目录中的配套 DLL，无需另建旧版包目录或 `STC.Frame/lib` 目录。

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

## 关注债券与筛选分组

行情侧栏的「关注债券」「筛选分组」旁点击 `+` 打开管理弹窗。信用、利率分开管理；报价管理等合并侧栏先选择市场。支持新建、双击重命名、删除；所有修改点击「保存」后写入当前登录账户，取消或关闭不提交。关注组可粘贴债券代码、识别后勾选加入，也可从行情行右键「添加到分组」。债券资料按明确代码查询，不扫描整个基础资料库。

分组使用现有 `ficc.watchlist.*`、`ficc.filterGroup.*` API，服务端生成 ID，以 `expectedRevision` 防止旧编辑覆盖新修改。未编辑的组不重复写入。批量弹窗按组逐项保存，不是整个弹窗的数据库原子事务；若部分失败，显示错误并重读服务端事实，保留编辑内容，不能当作全部成功。版本冲突需要重新打开后编辑。

点击侧栏分组标签启用/取消过滤：关注组之间 OR，筛选组之间 OR，两类与临时条件之间 AND；组内所有子分组拍平后同字段 OR、不同字段 AND。客户端发送分组 ID，由 QuoteSvr 在分页前执行，避免只过滤已加载的一页。报价管理共用账户分组，清除过滤只取消激活状态，不删除分组。

条件目录从对应侧栏与服务端字典生成并缓存，保存稳定字段、操作符及结构化值；禁用条件不能保存。产品 4.8.4 禁止的利率条件（地方债地区、主体评级、做市标的、永续债）在客户端加载/保存以及 QuoteSvr 加载/保存时清理。组合查询仍限定每个筛选组的所属市场。本次需要同时发布客户端和 QuoteSvr，common 没有源码修改。

管理窗口中的 `22-account-groups` 是本功能的桌面回归用例：真实 test2 登录、创建报价、识别债券、分组保存重读、联合过滤、取消、重命名、移除债券、版本冲突及确认删除，只清理自身数据。2026-10-13 本机实际运行通过 17 项检查；服务端 `AccountGroupSmoke` 的 10 项小样本功能检查通过。未运行压力测试，也未在远端 QA 验证。
