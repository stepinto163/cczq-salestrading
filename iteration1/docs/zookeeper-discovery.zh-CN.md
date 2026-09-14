# 本机 ZooKeeper 与 STC 服务发现

更新日期：2026-09-14。

## 1．安装与运行方式

已安装 Apache ZooKeeper 3.8.6，使用 Apache 官方二进制包并校验 SHA-512。安装目录为 `iteration1/runtime/tools/apache-zookeeper-3.8.6-bin`。

本机只运行一个 ZooKeeper 进程，监听 `127.0.0.1:12181`；Java 堆上限为 128 MB，进程总内存会包含堆外开销。未设置开机自启动，未修改其他 ZooKeeper 或系统数据库实例。这是开发注册中心，不是生产 HA 集群。

从项目根目录执行：

```powershell
# 首次安装；已安装时校验现有包
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/setup-zookeeper.ps1

# 启动及停止
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-zookeeper.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/stop-zookeeper.ps1
```

运行配置及数据位于 `runtime/zookeeper/`，输出日志为 `runtime/zookeeper.log`、`runtime/zookeeper.err`。停止脚本只停止记录的本项目进程，保留数据。完整启动顺序见 [本机启动指南](local-startup.zh-CN.md)。

安装版本及配置依据：[Apache 发布页](https://zookeeper.apache.org/releases/)、[3.8.6 管理文档](https://zookeeper.apache.org/doc/r3.8.6/zookeeperAdmin.html)。

## 2．服务配置

QuoteSvr、NotificationSvr 均通过 `ServerWrapper.Start` 启动，使用自身 `control/ATSConfig.ini` 中的定义注册到 `/MDTService/<ServiceName>`。

NotificationSvr 在 `resource.xml` 装配 `NetClientResource`。公共 `QuoteSvrClient` 使用逻辑键 `SERVER.QuoteSvr`，通过现有 `NetClientWrapper` 请求及订阅，由 STC 管理发现、连接和订阅恢复。

GW 的 QuoteSvr、NotificationSvr 路由也已启用 ZooKeeper 发现。

```ini
REGISTER.ServerList=REGISTER.Local
REGISTER.Local.Name=LocalZooKeeper
REGISTER.Local.Host=127.0.0.1:12181

SERVER.QuoteSvr.Name=QuoteSvr
SERVER.QuoteSvr.ServiceName=QuoteSvr
SERVER.QuoteSvr.Host=127.0.0.1:1
SERVER.QuoteSvr.RegType=1
SERVER.QuoteSvr.LBFactor=1
SERVER.QuoteSvr.RegisterEnable=1
SERVER.QuoteSvr.RegisterServerList=REGISTER.Local
```

上例是**调用方**配置。此版本 `PropertiesHelper` 即使启用注册仍强制要求 `Host`、`LBFactor`。调用方用不可用的 `127.0.0.1:1` 占位，实际地址来自注册中心；不存在固定地址回退。不要将该占位地址复制到服务自身的监听配置。

QuoteSvr 自身仍配置实际 `127.0.0.1:18680`，NotificationSvr 自身为 `127.0.0.1:18690`。如关闭注册使用固定地址模式，应将调用方 `Host` 改成真实地址。

## 3．身份与订阅边界

- 真实 LoginSvr 暂未启动；原来的模拟登录仍由 QuoteSvr 承载。因此调用方 `SERVER.LoginSvr.ServiceName=QuoteSvr`，发现的是 QuoteSvr 节点，不是一个新增 LoginSvr 进程。
- `SimulatorLoginSvrClient` 实现 `LoginSessionClient`，把模拟登录和用户解析从 NotificationResource 中分离。模拟账号在 `resourceCfg.ini` 显式配置，业务资源没有固定地址或账号。
- `QuoteSvrClient` 使用实例化的 `NetClientWrapper` 调用，显式传入 sessionId 和 3 秒超时；不修改 `BaseApi` 全局静态 sid，不另建底层网络连接。
- 普通报价事件和价格更新分别订阅。当前公共库 `GwClient.subscribeWithImage` 未向底层转发业务 sid，因此采用普通价格订阅加显式带 sid 的原生价格快照请求，避免绕过认证或复制错误实现。
- STC 管理网络恢复；NotificationResource 保留有界队列、连接代次、事件序号检测和业务状态重建。连接变化或队列溢出时进入未就绪状态，重新登录模拟服务并查询状态。
- 本轮没有修改用户提供的基础库原仓库，没有新增网络框架。真实 LoginSvr 适配以及完整生产主备切换仍是后续工作。

## 4．本轮验证

| 验证 | 结果与范围 |
|---|---|
| 注册与只读请求 | ZooKeeper 中存在 QuoteSvr、NotificationSvr 实例；C# 经 GW 完成报价、行情和提醒偏好查询，0 条新报价 |
| 修改服务端口 | 只修改 QuoteSvr 的监听配置到 18681；GW 和 NotificationSvr 不重启即可发现新地址，提醒服务重新同步，C# 只读请求通过；随后恢复 18680 |
| 提醒功能 | 20 项通过，覆盖四类提醒；3 条测试报价 |
| C# 界面功能 | 18 项通过；1 条测试报价，验证生命周期、行情和提醒更新 |

证据在 `runtime/service-discovery-*.json`、`runtime/service-relocated-*.json`、`runtime/service-relocation.json`、`runtime/zk-notifications.json`、`runtime/zk-ui.json`。消息全文仍保存到报告所指向的 `messageLogDirectory`。

```powershell
# 只读：注册信息及 C# 服务调用；不会创建报价
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-service-discovery.ps1
```

`test-registry-relocation.ps1` 会短暂重启 QuoteSvr 并恢复原端口，供专门的恢复检查使用，不纳入普通启动流程。强制停止 JVM 后，旧临时节点可能保留到 ZooKeeper 会话超时，本轮观察到新旧节点短暂同时存在。该检查不证明生产双活、数据复制或长时间注册中心中断后的恢复能力。
