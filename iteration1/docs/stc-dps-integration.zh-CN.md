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
| 本地 ZooKeeper `127.0.0.1:12181` | 未安装（`runtime/tools` 下只有未下载完的 `apache-zookeeper-3.8.6-bin.tar.gz.part`），需先跑 `scripts/setup-zookeeper.ps1` |
| `gateway/control/server.lic` | 脚本会从 `cczq-app-salestrading-quotesvr/control/server.lic` 复制 |

另外 `gateway/control/ATSConfig.ini` 里的 `REGISTER.Local` 指向本机 ZooKeeper，
且把 `QuoteSvr` / `NotificationSvr` / `LoginSvr` 都注册到本地（Host `127.0.0.1:1`，由注册中心覆盖）。
也就是说该配置描述的是「GW 作为本机前置网关、其余服务同机部署」的形态，
与当前这条「本机 QuoteSvr 直连 STC QA（10.130.21.53:2181）」的验证链路不是同一拓扑，
起服前需要先明确 GW 在本任务中扮演的角色。
