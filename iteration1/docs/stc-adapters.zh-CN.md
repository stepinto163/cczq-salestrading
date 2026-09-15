# STC 与 CRM 可切换依赖适配

本机开发默认使用模拟数据；内网联调通过配置接入 STC LoginSvr、BaseSvr。QuoteSvr、NotificationSvr 的业务代码使用同一组接口。CRM 可由内网团队独立编译为 JAR，替换配置即可接入。

## 1．实现与边界

| 依赖 | 配置 | 实现 |
|---|---|---|
| 用户与会话 | `identity.mode=simulator` | QuoteSvr 承载 `LocalLoginSimulator`，NotificationSvr 经 STC 客户端调用它 |
| 用户与会话 | `identity.mode=stc` | `StcLoginSessionClient` 调用复用源码的 `LoginSvrClient` |
| 债券、发行人、中债估值 | `base.mode=simulator` | `SimulatorBaseDataClient`，固定样例 |
| 债券、发行人、中债估值 | `base.mode=stc` | `StcBaseDataClient`，经 `NetClientWrapper` 查询并缓存真实数据 |
| CRM | `crm.mode=simulator` | 固定机构、联系人样例 |
| CRM | `crm.mode=empty` | 空列表；按 ID 查询返回空 |
| CRM | `crm.mode=custom` | 独立 JAR 实现 `CrmClient` |

三类依赖可以分别切换。真实模式失败时不会回退到模拟数据。业务仍使用 `ReferenceData` 与 `LoginSessionClient`，不直接依赖供应商字段。

### LoginSvrClient 源码复用

已有源码：`D:/workspace/GitHub/sc-app-stc/src/main/java/sc/app/stc/utils/LoginSvrClient.java`。

当前源码使用 `initialize(String, NetClientWrapper)`；平台打包的旧 STC JAR 使用 `initialize(String, CedaClientWrapper)`。因此本轮把已有源码编入 common 模块的 `ficc.common.stc` 包，避免旧 JAR 同名类覆盖，并将请求超时改为可配置。不是重新实现一套用户缓存或登录协议，也没有要求升级全部基础框架。移植文件与差异说明见 [源码说明](../cczq-app-salestrading-common/src/main/java/ficc/common/stc/README.zh-CN.md)。

客户端经 GW 登录真实 LoginSvr；业务服务用请求的 SID 解析 STC `userid`，所有权与审计保存该真实 ID。客户端填写的操作人不能替代已验证会话。管理员和内部服务账号分别通过 `identity.adminUserIds`、`identity.internalUserIds` 明确授权，不按 `S-` 等 ID 前缀推断权限。

注销、用户更新及连接变化会清理登录缓存，并使相关业务订阅失效；客户端重新登录后需重新打开订阅。真实模式下 QuoteSvr 自带的模拟登录入口关闭。

### BaseSvr 首轮接入

| STC 请求主题 | 请求示例 | 读取内容 |
|---|---|---|
| `querySecurity` | `{"SecurityID":"250001.IB"}` | `datas` 中的 `BondInfoView`，包括债项评级、主体评级、发行人、条款类型 |
| `stc.base.info.query` | `{"TableName":"p_issuer_info","KeyValue":"实际发行人代码"}` | `Datas` 中的发行人名称、类型、地域 |
| `queryChianSecurity` | `{"SecurityID":"250001.IB"}` | `datas` 中的中债估值、估值日期与剩余期限 |

保留 STC 原主题拼写 `queryChianSecurity`。债券代码按 STC 的 `CIB→IB`、`SSE→SH`、`SZE→SZ` 规则转换并校验。利率债/信用债通过显式配置映射，不用交易场所推断；发行人类型也由配置映射。

单后台线程刷新配置的债券集合，默认间隔 60 秒；一轮成功后整体替换缓存。筛选及报价处理读本机缓存，不逐条远程查询。收到 `stc.base.clear`、`stc.base.manualClear.**` 或连接变化后使缓存失效，下一轮刷新恢复。未就绪或刷新失败时查询返回不可用错误。

这是内网联调首版：`base.securityIds` 为显式债券集合（1～1000 个），应覆盖已有 QA 报价；建议先配少量真实债券。它尚不是全市场基础数据同步器。查询使用刷新后的评级和最新可得日估值；已打开视图尚未实现基础数据变化单独触发全量重算，验证属性变化时应重新查询。

中证估值暂未接入：已检查的 `queryCsiBondValue` 返回值缺少可靠估值日期，不能填造日期或把多个值当作一个值。CalSvr 计算、真实 QT 接入用户映射也不在本次适配范围。Base 适配复用了 STC 网络封装及已核对的请求契约，尚未整体移植 `BaseSvr2Client`。

## 2．内网配置步骤

1. 构建并部署新的 Maven 服务包，参阅 [Maven 构建说明](maven-jenkins.zh-CN.md)。不要混用旧 `target` 与新的 `target-maven` 产物。
2. 复制 [内网适配配置模板](../config/adapters.integration.example.properties) 为部署机本地文件，例如 `D:/services/salestrading/adapters.local.properties`。设置真实债券代码、字典映射及服务账号。
3. 在 QuoteSvr、NotificationSvr 各自 `control/<服务名>/resourceCfg.ini` 中设置：

```ini
adapter.properties=D:/services/salestrading/adapters.local.properties
```

数据库配置仍由 `quote.properties` 指向；先加载它，再用适配文件覆盖同名配置。路径相对进程工作目录解析，部署时建议用绝对路径。配置文件按 UTF-8 读取。

4. 将 [服务消费者配置](../config/stc-consumers.example.ini) 合并到相应服务的 `ATSConfig.ini`，配置内网 ZooKeeper。调用方以服务键寻址，不能把模板的 `127.0.0.1:1` 当成服务监听地址。真实 provider 名称须与内网注册信息一致。
5. GW 的 `SERVER.LoginSvr.ServiceName` 从本机模拟使用的 `QuoteSvr` 改为真实 `LoginSvr`；NotificationSvr 的同项也改为真实服务。GW 仍独立运行，使用其原有登录流程。
6. NotificationSvr 使用专用 STC 服务账号登录并读取内部报价/价格流。填写 `identity.serviceUsername`，把该账号的真实 `userid` 加入 QuoteSvr 的 `identity.internalUserIds`。密码从 `identity.servicePasswordEnv` 指定的环境变量读取；启动进程须继承它。普通用户无需加入内部账号列表。
7. 重启相应服务，使用真实账号经 GW 登录。可先通过 DPS API 调试器查询基础资料及创建一条报价，检查返回值和数据库中的实际操作人，再验证 NotificationSvr 的内部订阅。

本机无需修改配置即可继续模拟运行；显式配置见 [本机模板](../config/adapters.local.example.properties)。切回模拟模式还需把 GW、NotificationSvr 的 LoginSvr 服务发现目标改回模拟服务。不要对正在运行的进程热切换模式。

C# 平台的固定样例账号、债券选择尚未完整改造成内网业务界面；本轮内网适配验证可使用支持输入地址和账号的独立 DPS API 调试器。模拟自动测试使用固定账号与数据，不可直接当作真实环境验收脚本。

## 3．CRM 独立实现

空模式没有假机构或假联系人。待确认报价仍能保存手工输入的机构/联系人名称；确认时，已输入名称必须匹配可访问的真实 ID，所以空 CRM 无法完成这类报价的确认。

内网团队从 [CRM 独立工程](../extensions/crm-adapter/README.zh-CN.md) 开始，实现 `CrmClient`，编译 JAR 后配置：

```properties
crm.mode=custom
crm.adapterClass=cczq.salestrading.crm.InternalCrmClient
crm.adapterJar=D:/services/salestrading/extensions/cczq-salestrading-crm-adapter-0.1.0-SNAPSHOT.jar
crm.serverKey=SERVER.CrmSvr
```

适配器接收现有 `NetClientWrapper` 和配置，沿用 STC 服务发现。需要时自行增加 CRM 的消费者配置。主服务不硬编码 CRM 消息格式。更换 JAR 后重启服务，无需修改报价生命周期代码。

## 4．本轮验证（2026-09-14）

- Maven 全模块构建通过；9 个单元/契约测试通过，包括已有筛选测试、复用 LoginSvrClient 源码的请求与缓存测试、基础资料映射和失效、真实用户权限，以及外部 CRM JAR 的编译与加载。
- CRM 独立工程构建通过；C# 平台构建通过。
- 仅使用 3 条报价，经真实 C# DPS → GW → QuoteSvr → PostgreSQL 完成 40 项功能检查；独立临时 schema 已删除。没有执行压力测试。
- 本轮未连接内网真实 LoginSvr/BaseSvr/CRM；真实返回数据、字典、权限和网络重连需要内网集成验证。NotificationSvr 已接入适配接口，并按实际用户集合生成提醒；本轮三条报价的端到端测试未启动 NotificationSvr。

本机功能记录：`iteration1/runtime/adapter-smoke/20260914_162657_5f448878/functional.json`。运行日志和凭据不进入版本库。
