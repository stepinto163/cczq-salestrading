# STC 服务发现

服务使用 STC 的标准启动、注册和客户端资源，通过 ZooKeeper 发现配置的目标服务，不在业务代码中写死远端 IP。机制依据见[STC 开发约定](../../docs/stc-development-principles.zh-CN.md)。

## 配置归属

- 注册中心地址及 `SERVER.*` 服务定义由对应环境的 `ATSConfig.ini` 维护。
- QA 使用共享 `$ATS_ROOT/control/ATSConfig.ini`；服务包不覆盖它，见[QA 部署](jenkins-qa.zh-CN.md)。
- 仓库内的启动脚本有独立本机配置，完整本机 STC 环境位于外部 `qa_env`；两套部署及脚本的范围见[启动说明](local-startup.zh-CN.md)。端口以各环境配置为准。
- `SERVER.LoginSvr` 必须指向当前选择的真实或模拟登录服务。模拟模式与 STC 模式的切换见[依赖适配](stc-adapters.zh-CN.md)。

## 内部订阅

NotificationSvr 通过 `QuoteSvrClient` / `NetClientResource` 接入 QuoteSvr，分别订阅报价事件与价格快照，再请求内部状态完成恢复。此链路不执行用户登录；前端业务请求仍须验证真实用户会话。

连接中断、代次变化或有界队列溢出会触发业务状态重建。连接恢复和资料就绪必须根据实际日志确认，不能仅凭客户端资源初始化成功判断。会话失效后，前端需重新登录并重新打开视图。

## 排障

1. 确认进程实际读取的 ATS_ROOT、工作目录及配置路径。
2. 检查注册中心可达、服务注册名与消费者配置一致，注册的地址可从调用方访问。
3. 比较进程日志与 ZooKeeper 节点；端口仍监听不代表临时注册节点仍存在。
4. 检查发现、连接、请求及恢复日志，日志位置见[组件日志](component-logging.zh-CN.md)和[DPS 日志](dps-logging.zh-CN.md)。

旧临时节点可能到会话超时后才消失；单次本机重连成功不代表长期中断、主备切换或双活已经验收。旧原型的服务发现测试脚本已移除，不再作为当前运行入口。

## 已知缺陷：会话过期后进程仍运行，但注册节点未恢复

**记录日期：2026-10-03。状态：已确认原因，永久修复暂缓。** 本节记录实际排查结果和后续验收要求，不表示恢复逻辑已经修复。

### 现象与证据

QuoteSvr 进程仍运行并输出心跳，但 QtIntakeSvr 报告 `SERVER.QuoteSvr` 不在线。检查 ZooKeeper 时，`/MDTService/QuoteSvr` 的子节点为 `[]`，所以不能用进程存在、端口监听或业务心跳判断服务发现正常。

本机日志时间线如下：

| 时间 | 证据 |
| --- | --- |
| 2026-09-30 19:12:23.600 | QuoteSvr 首次创建注册节点，地址为 `127.0.0.1:28680`。 |
| 2026-10-01 14:12:17.568 | ZooKeeper 客户端报告超时，距上次收到服务端响应 `12964370ms`。 |
| 2026-10-01 14:12:19.457 | 原会话过期（`Expired`）。 |
| 2026-10-01 14:12:19.477–487 | 新会话建立，出现 `SyncConnected` 和 QuoteSvr 子节点变化回调，但未重新创建注册节点。 |
| 2026-10-03 约 08:43 | 实际查询 `/MDTService/QuoteSvr`，子节点仍为空。 |
| 2026-10-03 重启 QuoteSvr 后 | 子节点恢复为 `QuoteSvr0000000014`，QtIntakeSvr 随后连接成功。 |

证据保存在本机环境，未作为仓库交付文件：

- 原进程日志：`qa_env/ats_root/log/QuoteSvr.20261003-084439-429.console.log`（`qa_env` 位于仓库旁）。
- 部署依赖：`qa_env/ats_root/stc/QuoteSvr/javalib/sc-app-common-0.1.0-SNAPSHOT.jar`。
- 对部署 JAR 执行 `javap -p -c` 的结果：`iteration1/runtime/zkcluster-deployed-bytecode.txt`。

长时间通信中断的触发原因尚未确认，不能据此断言是电脑休眠。会话过期后的注册恢复缺陷已通过源代码和实际部署 JAR 字节码交叉确认。

### 根因

问题位于共享 STC 依赖的 `com.app.common.cluster.ZkCluster`，参考源码为 `D:\workspace\GitHub\sc-app-common\src\main\java\com\app\common\cluster\ZkCluster.java`。

1. 首次启动调用 `createServerNode(serverInfo, true)`，创建临时顺序节点。
2. 注册节点消失后的子节点回调调用 `createServerNode(serverInfo, false)`。
3. 在本次运行使用的非主备分支中，实际创建节点的代码受 `if (isFrist)` 控制；恢复调用传入 `false`，跳过创建。
4. 方法仍返回成功，业务进程继续运行，形成“进程在线、注册缺失”的状态。

此结论针对已核实的非主备路径；主备分支的选主和恢复行为需要另行验证。QtIntakeSvr 的高频等待日志暴露了问题，但不是注册丢失的根因。

### 临时恢复与当前边界

已通过管理脚本重启 QuoteSvr 恢复本次注册：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ..\qa_env\ats_root\scripts\service.ps1 -Action restart -Service QuoteSvr
```

上述命令从仓库根目录执行。重启后须同时确认注册节点存在、注册地址正确、消费者连接成功；单看脚本输出“已启动”不足以验收。重启只是临时恢复，不能防止下次会话过期后复发。

QtIntakeSvr 的连接等待日志已改为状态变化时输出，重试仍继续。恢复连接后另有 `OwnQtNO=2881631456` 缺少 STC 用户映射的问题，待发送请求仍被保留；恢复注册不等于转发业务已经全部成功。

### 后续修复范围与验收

修复共享注册组件时，应将业务启动与注册节点维护分开：业务只启动一次，新会话建立后可重新注册；重复恢复应保持幂等。未成功创建节点时不能返回注册成功，也不能通过重复启动业务造成端口冲突或重复处理器。保留现有主备选主语义。

在隔离环境验证以下场景，不直接让日常使用的共享 ZooKeeper 会话过期：

1. 服务首次启动成功注册，消费者可连接并完成请求。
2. 强制会话过期后，不重启业务进程即可重新注册，消费者自动重新发现并完成请求。
3. 连续多次断开、过期与恢复，不生成重复注册节点，不重复启动业务或绑定端口。
4. 注册失败时输出明确错误和原因，恢复成功时输出明确状态；注册失败不得显示为就绪。
5. 单独验证主备切换，确认不会破坏选主和服务唯一性。

本次只记录缺陷，未修改共享 STC 注册代码，也未执行上述故障注入回归。
