# 接入真实 STC：NetType 必须是 dps

适用对象：本机原型（QuoteSvr / NotificationSvr / gateway）访问真实 STC 服务（LoginSvr / BaseSvr）。

## 1. NetType=dps（不是 ceda，也不是 gw）

`sc.app.stc.net.NetClientResource` 只有两条分支：

```java
String netType = Consts.getNetType();
if ("ceda".equals(netType)) { ceda = new CedaClientResource(); } else { gw = new GwClientResource(); }
```

QA 环境的 STC 服务端（BaseSvr / LoginSvr）跑的是 **DPS/GW 协议**，因此客户端必须落在
`else` 分支。取值用 **`dps`**（与 STC 仓库中 17 份 `ATSConfig.ini` 及现场 QA 配置一致）；
`gw` 虽然走同一分支，但语义上不是现场使用的网络类型，不要用它。

### 判别方法（关键）

`dps` 模式下 `com.gw.common.utils.GwClient` 会打印下列日志，出现 `CONNECTION_LOGIN_SUCCESS`
即代表握手成功：

```
GwClient zkClient: SERVER.BaseSvr
com.app.common.cluster.ZkCluster - currentChilds:/MDTService/BaseSvr/BaseSvr,BaseSvr,Host:10.130.21.53,Port:20041,Stat:1
GwClient connecting to Host:10.130.21.53,Port:20041
GwClient - Host:10.130.21.53,Port:20041:CONNECTION_LOGIN_SUCCESS
GwClient connected to Host:10.130.21.53,Port:20041
```

排查时必须先把 `log4j.logger.com.gw=DEBUG`、`log4j.logger.com.app=DEBUG` 打开——
默认 `log4j.rootLogger=WARN`，GW 客户端的 INFO 日志是看不见的，容易误判成“客户端没启动”。

### 为什么 ceda 不行

`ceda` 会创建 `CedaClientResource`，向同样的 `10.130.21.53:20040 / 20041` 发起 CEDA 握手，
但服务端不讲 CEDA 协议，结果恒为：

```
ats.app.ym.ceda.CedaClient - CedaClient connecting to 10.130.21.53:20041
ConnectionException: no reply
CEDA-api-4.0.1-SNAPSHOT Build: 20210330
```

即 TCP 可连、服务端不回包。这是协议不匹配，不是网络问题。

## 2. dps 模式下的初始化时序

`GwClientResource.start()` 才创建 `GwClientWrapper`，而 `NetClientWrapper` 是在 `init()` 里
new 出来的。因此调用方必须在资源 **start 之后** 再调用 `NetClientWrapper.init(serverKey)`，
否则 `gwClientWrapper` 仍为 null，`init()` 静默空转，后面 `isOnline()` 永远是 false。

QuoteSvr / NotificationSvr 都在 `start()` 里通过
`Server.getServer().getResource("NetClientResource")` 取资源，因此满足该约束。

另外启动瞬间会有一条 `SERVER.BaseSvr is unavailable`：此时连接尚未建立，属于正常竞态，
下一个刷新周期（默认 60s）即恢复。

## 3. BaseSvr 应答字段大小写

STC BaseSvr 返回的数据行在 **`Datas`**（首字母大写）：

```json
{"Msg":"Success","Datas":[{"Bond_ID":"250006.IB","Bond_Subtype":"BGB", ...}],"Code":"0000"}
```

客户端解析必须按 `Datas` 取（历史代码里写成小写 `datas` 会永远读不到数据）。
`StcBaseDataClient.rows()` 现在同时接受两种写法。

## 4. ceda 模式才需要的 JDK 17 兼容补丁（当前不使用）

若将来真要切到 `NetType=ceda`，JDK 17 下还需两个补丁，已保留在
`scripts/ceda-jdk17-shim/`（本文档不再展开）：

| 引用方 | 缺失类 |
|---|---|
| ceda-api（com.adaptiveMQ2） | `sun.misc.BASE64Encoder` |
| ats-app-ym-foundation（TripleDes） | `sun.misc.BASE64Decoder` |

运行时还需 legacy Netty 3：`F:\dev\maven-repo2\io\netty\netty\3.7.0.Final\netty-3.7.0.Final.jar`。

## 5. gateway 模块（GW）的构建与启动前置条件

`iteration1/gateway` 的 4 个类（`FiccSecurityCheck`、`MeasuredCedaProxy`、
`MeasuredTcpConnector`、`PerformanceTrace`）继承自旧版 GW 二进制里的
`com.gateway.connector.tcp.TcpConnector` / `com.gateway.invoke.*` / `com.gateway.message.*`，
Nexus 上的 `gateway-api` 并不包含这些类，因此该模块默认不在 reactor 里，必须显式开 profile：

```powershell
& "E:\Program Files\apache-maven-3.9.16\bin\mvn.cmd" `
  -s "E:\Program Files\apache-maven-3.9.16\conf\settings_yanxin.xml" `
  -f E:\gongy\projects\cczq-salestrading\iteration1\pom.xml `
  -P with-gateway -DskipTests clean package
```

旧版 GW 二进制现在有源码可循：仓库根目录 `gw/`（来自 `stc-lite` 的 `gw` 工程），
`groupId=sumscope.stcl`、`artifactId=gw`、`version=0.1.1-SNAPSHOT`，入口类为默认包的 `GW`。
它依赖 Spring Boot 2.6.6 / Netty 4，**必须用 JDK 8 编译**，再 install 到本地仓库
（`F:\dev\maven-repo2`，由 `settings_yanxin.xml` 的 `localRepository` 指定）。
install 之后上面的 `-P with-gateway` 构建即可通过，产物为
`iteration1/gateway/target-maven/cczq-app-salestrading-gw-0.1.0-SNAPSHOT.jar`
（注意：本仓库把 `maven.build.directory` 改成了 `${project.basedir}/target-maven`，
不是默认的 `target/`）。

`scripts/start-gateway.ps1` 还有三个运行时前置条件，未满足前不要尝试起服：

| 前置条件 | 当前状态 |
|---|---|
| `runtime/foundation-build/lib/gw-local.jar` | 不存在；`scripts/foundation-classpath.ps1` 会回退到 `runtime/foundation-classpath.txt`（其中已含 `gw-0.1.1-SNAPSHOT.jar`） |
| 本地 ZooKeeper `127.0.0.1:12181` | **已解决**：见第 6.1 节，`scripts/setup-zookeeper.ps1` 已修复下载源并可正常安装 |
| `gateway/control/server.lic` | 脚本会从 `cczq-app-salestrading-quotesvr/control/server.lic` 复制 |

另外 `gateway/control/ATSConfig.ini` 里的 `REGISTER.Local` 指向本机 ZooKeeper，
且把 `QuoteSvr` / `NotificationSvr` / `LoginSvr` 都注册到本地（Host `127.0.0.1:1`，由注册中心覆盖）。
也就是说该配置描述的是「GW 作为本机前置网关、其余服务同机部署」的形态，
与当前这条「本机 QuoteSvr 直连 STC QA（10.130.21.53:2181）」的验证链路不是同一拓扑，
起服前需要先明确 GW 在本任务中扮演的角色。

## 6. 本机 GW 起服实测与「自注册 vs 消费 STC」冲突（2026-09-15）

### 6.1 本地 ZooKeeper 已可安装

`scripts/setup-zookeeper.ps1` 原来只从 `downloads.apache.org` 下载，而该站只保留当前版本，
3.8.6 已移到归档站，脚本因此必然 404。现已改为**依次探测** `archive.apache.org` →
`downloads.apache.org`，取第一个可用的镜像，校验 SHA-512 后解压到
`runtime/tools/apache-zookeeper-3.8.6-bin`。之后 `scripts/start-zookeeper.ps1`
可正常拉起 `127.0.0.1:12181`（`ruok` 返回 `imok`）。

> 排障提示：若 `Start-Process` 报「已添加项。字典中的关键字:"Path"所添加的关键字:"PATH"」，
> 说明当前进程环境里同时存在 `Path` 与 `PATH`（例如从 Git Bash 启动会话时会带入），
> 此时 PowerShell 枚举 `Env:` 即失败。在会话开头执行一次
> `Remove-Item Env:\PATH -ErrorAction SilentlyContinue` 即可恢复。

### 6.2 GW 二进制可起服，并与 QuoteSvr 建立真实 DPS 会话

在 QuoteSvr 已连上 STC QA 的前提下执行 `scripts/start-gateway.ps1`：

- `127.0.0.1:18600` 进入 LISTENING；
- 日志出现 `* LoginSvr onConnected,subscribe:[SYS.ATS.LOGIN]` 与
  `* QuoteSvr onConnected,subscribe:[ficc.price.report.**, ficc.quote.report.**]`；
- `netstat` 显示 GW → `10.100.151.202:18680` 的 ESTABLISHED 连接，
  QuoteSvr 侧持续出现 `cmd=2` 心跳双向收发。

即 **GW ↔ QuoteSvr 这一段是真实跑通的**；目前只差最后一环：能连 `18600` 的 Windows
客户端（`iteration1/client` 目标框架为 `net472`，本机尚未构建）。

### 6.3 同一进程内「自注册」与「消费 STC」互斥（重要）

要让 GW 从本地注册中心发现 QuoteSvr，需要 QuoteSvr 作为 provider 注册。实测三种配置变体，
**只要打开 `SERVER.QuoteSvr.RegisterEnable=1`，消费侧就立刻失效**：

| 变体 | `REGISTER.ServerList` | `SERVER.QuoteSvr` | 结果 |
|---|---|---|---|
| 基线（可用） | `REGISTER.Svr1` | `RegType=2 / RegisterEnable=0` | `CONNECTION_LOGIN_SUCCESS` ×2（20040、20041） |
| A | `REGISTER.Svr1,REGISTER.Local` | `RegType=1 / RegisterEnable=1` | BaseSvr 解析为 null |
| B | `REGISTER.Local` | `RegType=1 / RegisterEnable=1` | BaseSvr 解析为 null |
| C | `REGISTER.Local,REGISTER.Svr1` | `RegType=1 / RegisterEnable=1` | BaseSvr 解析为 null |

三种变体下 QuoteSvr 都能启动、都能往本地 ZK 写入
`/MDTService/QuoteSvr/QuoteSvr = Host:<本机IP>,Port:18680,Stat:1`，
但 `SERVER.BaseSvr` / `SERVER.ValidationServer` 的 `ZkCluster` 拿不到节点，
刷新时报 `BaseSvr reference refresh unavailable: SERVER.BaseSvr is unavailable`；
改回基线后 2 条 `CONNECTION_LOGIN_SUCCESS` 立即恢复 —— 因果明确，不是 STC 侧波动
（同期用只读 ZK 探针直连 `10.130.21.53:2181`，`BaseSvr` 与 `ValidationServer` 均 ONLINE）。

结论：**当前 foundation 客户端无法让一个进程同时「向本地注册中心自注册」和
「从 QA 注册中心发现 STC 服务」**。要同时满足两者只有两条路：

1. 由外部（例如一个独立的本地注册脚本）往本地 ZK 写入 QuoteSvr 节点，
   QuoteSvr 自身保持 `RegisterEnable=0`；
2. 或让 GW 走固定地址（`Host` 直接写 `10.100.151.202:18680`）而不依赖注册中心 ——
   任务约束允许在本机配置副本里关闭注册发现做固定地址诊断。

这也是此前「GW 拓扑角色待定」的技术根因，需要发起方在两种方案中选一个。

### 6.4 客户端侧：DPS 请求/应答已通，只差 STC 账号

用仓库自带的 DPS 客户端 `com.gateway.connector.tcp.client.GateWayApi`
（即 `compat/tests/StcTransportSmoke.java` 所用的那个）写一个**只读**探针，
连到 QuoteSvr 实际监听地址 `10.100.151.202:18680`（注意：QuoteSvr 绑定的是本机网卡地址，
**不是 127.0.0.1**，连 127.0.0.1:18680 会静默无应答），只发 `ficc.base.get`，不创建或修改任何报价：

- TCP 建连成功；
- `SYS.ATS.LOGIN` 收到真实业务应答：`{"Code":"1003","Msg":"Local login simulator is disabled; use STC LoginSvr"}`。

即 **客户端 → 服务端的 DPS 请求/应答链路本身是通的**；登录被拒的唯一原因是
`identity.mode=stc` 时本地登录模拟器被显式关闭，必须提供真实 STC 账号。
这把此前「真实登录做不了」的模糊阻塞，收敛成了一个明确的、只依赖外部凭据的点。

同一探针走 GW 前置 `127.0.0.1:18600` 则无应答：

| GW 配置 | 现象 |
|---|---|
| 默认（本地 ZK 发现） | `SERVER.LoginSvr is online:false` —— 本地 ZK 里没有 provider 实例 |
| 本机固定地址副本（`Host=10.100.151.202:18680`、`RegType=0`、`RegisterEnable=0`） | GW 能起服、18600 监听，但仍不路由 |

即 `RegType=0` **不是**该 foundation 的固定地址开关，GW 前置仍需按 6.3 的两种方案由发起方定夺。

## 7. 根因补全：ZkCluster.zk 是进程级静态单例（2026-09-15）

6.3 记录的「自注册与消费 STC 互斥」此前只有现象，没有源码级解释。读 stc-lite 源码后已定位：

- `sc-app-common/.../com/app/common/cluster/ZkCluster.java:29` —— `public static ZookeeperClient zk = null;`
- `createZkClient()`（同文件 47-74 行）是 `synchronized`，且**只在 `zk == null` 时才创建**。

因此**一个 JVM 进程内只能存在一套 ZooKeeper 连接**：谁先调用，后面所有服务（无论消费还是注册）
都复用同一个 `zk`。QuoteSvr 先消费 STC QA（10.130.21.53:2181）拿到 `zk`，
随后它向本地 ZK（127.0.0.1:12181）自注册时 `zk` 已非空，节点被写到**错误的** ensemble，
`SERVER.BaseSvr` / `SERVER.ValidationServer` 自然解析为 null。这与 RegType、NetType、端口都无关。

顺带纠正：`RegType` 不是「注册中心 / 固定地址」开关，而是**集群模式**——
`ServerInfo.TYPE = { "ALL", "ALL", "STANDBY" }`（0/1=ALL，2=STANDBY），
只影响创建 EPHEMERAL 还是 EPHEMERAL_SEQUENTIAL、以及监听 children 还是监听单节点 data。

### 7.1 解法：外部脚本写本地 ZK（已实测可行）

因为 `zk` 是**进程级**静态量，把 GW 与 QuoteSvr 放在**两个进程**即可各用一套 ZK；
此时只要有人把 QuoteSvr 的真实地址写进本地 ZK，GW 就能发现它，
而 QuoteSvr 可以继续 `RegisterEnable=0` 消费 STC QA。节点格式与 foundation 写的一致：

```text
/MDTService/QuoteSvr/<任意子节点名>  =  Host:10.100.151.202,Port:18680,Stat:1
```

实测（本地 ZK 127.0.0.1:12181 已起）：

- 用 ZK 自带 jar 建上述持久节点（子节点名 `QuoteSvrSeed`）；
- QuoteSvr 以基线配置（`REGISTER.ServerList=REGISTER.Svr1`、RegType=2、RegisterEnable=0）起服，
  `QuoteSvr READY`，监听 10.100.151.202:18680，仍照常消费 STC QA；
- GW 以 `RegType=0 / RegisterEnable=0` 的配置副本起服，日志出现
  `* QuoteSvr onConnected,subscribe:[ficc.price.report.**, ficc.quote.report.**]`
  —— 说明 **GW 通过外部写入的节点发现了 QuoteSvr，且 QuoteSvr 未牺牲 STC 消费能力**。

6.3 的「互斥」因此有了明确解法：**外部注册 + 双进程**，不需要发起方二选一。

### 7.2 GW 路由表必须来自 node_server_*.xml（此前缺失）

GW 起服后若日志出现：

```text
initNodeClientWrapper nodeId:* servers:null
```

说明 `CedaApi` 的后端路由表是空的，**所有请求会被静默丢弃**（客户端 `requestSync` 返回 null，GW 侧无任何日志）。
原因是 `CedaProxy` 的 `nodeFilePath` 属性没有注入——上游 `spring-ceda-client.xml` 里这一行是被注释掉的：

```xml
<!-- <property name="nodeFilePath" value="./config/nodes/"/>-->
```

修复：`iteration1/gateway/config/spring-config.xml` 的 `proxy` bean 增加
`<property name="nodeFilePath" value="./config/nodes/"/>`，
并提供 `config/nodes/node_server_local.xml`（`NodeId` 用 `*`，
`Servers` 列出 `LoginSvr` / `QuoteSvr` 及其 `RequestTopics`）。
加完后日志变为 `initNodeClientWrapper nodeId:* servers:{...}`，路由表已装载。

> 注意：路由表装载**不等于**端到端打通。本次实测该配置下 GW 仍不回包，
> 客户端 `requestSync` 依旧返回 null 且 GW 侧无任何请求日志，
> `cedaClientWrapperMap` 中 `onlineStatus` 仍为 `{}`。下一步需开 `com.gateway` 的 DEBUG 日志确认
> 请求是在 security check 还是 onlineStatus 判定处被丢弃。

### 7.3 登录返回值取决于 ATS_ROOT 能否读到 adapters.local.properties

直连 QuoteSvr `10.100.151.202:18680` 的 `SYS.ATS.LOGIN`：

| ATS_ROOT | 应答 |
|---|---|
| 能读到 `adapters.local.properties`（`identity.mode=stc`） | `Code=1003` `Local login simulator is disabled; use STC LoginSvr` |
| 读不到（例如 ATS_ROOT 指向 `runtime/quotesvr`，该文件在 `runtime/` 下） | `Code=0001` `Fixed LoginSvr simulator`，`sid=SIM-...` |

后者日志里会出现 `GwServerResource: handler=LoginSimulatorHandler,topic=SYS.ATS.LOGIN`。
**`Code=0001` 是本地模拟器身份，不是真实 STC 登录**，不能作为验收项 3 的证据；
真实登录仍需 STC 测试账号。
