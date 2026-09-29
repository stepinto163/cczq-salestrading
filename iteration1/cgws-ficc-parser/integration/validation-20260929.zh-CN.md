# Java 默认引擎与 STC 客户端验收

日期：2026-09-29；环境：Windows、本机真实 STC 服务、Java 8。Java 默认配置已同时覆盖 Windows 和 Linux salt 模板。

| 检查 | 结果 |
|---|---|
| Java 核心 Maven 构建 | 221 项通过 |
| ParserSvr 全主源码编译、服务合同及 CRM 协议 | 12 项通过；包括异步比较日志、子进程错误后继续处理 |
| PostgreSQL 只读检查 | 两个实际债券通过 |
| 实际 STC 客户端端到端 | 33 项通过，最终部署后再次通过 |
| 客户端部署与回滚 | 通过；备份迁移到插件扫描目录外，失败可恢复 |
| 原生比较 | 实际 DLL 完成初始化，两侧完整输入/输出及 DIFFERENT 已写日志 |
| Linux 运行 | NOT_RUN；本机无 Linux/WSL，配置已修改不等于运行验收 |
| 外部服务 Maven 发布包 | 未完成；依赖下载触发 50 秒看门狗。本机采用 QA JAR 编译、打 JAR、部署通过 |

端到端覆盖：真实登录、精确报价识别、机构/联系人匹配、双边与分号三条结果、多行及中文数量、勾选确认、报价持久化、手工待确认报价、行情与报价管理推送、修改、Refer/Unrefer、历史、本人最优报价提醒。四条测试报价已删除，提醒偏好已恢复。测试未覆盖全部提醒类型、模糊报价、外部 QTrade/FIX 或缺少本机库的 CalSvr。

实测输入：

```text
长城证券 田老哥
240210 2.5/2.4 1000;240215 2.3 2000 bid
```

Java 返回三条：240210 Bid 2.5/1000、240210 Ofr 2.4/1000、240215 Bid 2.3/2000；实际 C++ 返回一条 240210 Bid 2.5/2000。客户端收到 Java 三条结果，符合用户明确要求纠正旧行为。其他 DIFFERENT 也可能来自代码大小写等字段差异，不能全部视为缺陷。

本轮普通启动观察：Java 全量参考快照约 5.6 秒；原生比较进程约 19 分钟。不是负载基准，不据此保证其他机器耗时。原生慢启动不会阻塞 Java 回复。

复现客户端检查（仓库根目录，先确保本机服务已就绪）：

```powershell
$env:PARSER_TEST_USER = '<STC账号>'
$env:PARSER_TEST_PASSWORD = '<密码>'
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/stc-client/stc-client/scripts/test-parser-live.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/tests/test-client-deploy-layout.ps1
```

客户端脚本有 150 秒进程看门狗，会创建并清理测试报价；报告和截图保存在 `iteration1/stc-client/stc-client/runtime/native-client/parser-e2e-*`。服务检查证据在模块 `target/service-check/20260929-200606-814535/`。运行证据不提交 Git，避免携带会话及业务数据。

发现并修复：旧客户端部署将 `.deploy` 备份放在安装目录内，STC 递归扫描 DLL 时加载重复插件。现将客户端备份放到同级 `native-client-deployment-backups`，保留旧备份并验证失败回滚。服务部署目录规则保持原样。
