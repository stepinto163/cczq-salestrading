# 在 JDK 17 上运行 STC/CEDA 客户端

适用对象：本机原型（QuoteSvr / NotificationSvr）需要访问真实 STC 服务（LoginSvr / BaseSvr）时。

## 1. NetType 必须是 ceda

`sc.app.stc.net.NetClientResource` 按 `NetType` 选择客户端实现：

```java
String netType = Consts.getNetType();
if ("ceda".equals(netType)) { ceda = new CedaClientResource(); } else { gw = new GwClientResource(); }
```

只有 `ceda` 会创建 CEDA 客户端（`/MDTService/...` 上的 STC 内部协议）；其它任何取值（含 `gw`、`dps`）
都走 GW 客户端，`NetClientWrapper.isOnline("SERVER.BaseSvr")` 恒为 false，表现为：

```
BaseSvr reference refresh unavailable: SERVER.BaseSvr is unavailable
```

该报错与网络、注册中心、RegType 都无关，仅由 NetType 决定。

## 2. JDK 17 需要两个兼容补丁

CEDA 客户端（com.adaptiveMQ2）与 ATS 基础库（TripleDes）仍引用 JDK 8 的内部类：

| 引用方 | 缺失类 | 现象 |
|---|---|---|
| ceda-api（com.adaptiveMQ2） | `sun.misc.BASE64Encoder` | `NoClassDefFoundError`，客户端线程直接退出 |
| ats-app-ym-foundation（TripleDes） | `sun.misc.BASE64Decoder` | 同上 |

两者在 JDK 9 被移除。解决方式是用 `scripts/ceda-jdk17-shim` 下的源码编译同名类，再注入
`jdk.unsupported` 模块：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\iteration1\scripts\ceda-jdk17-shim\build-shim.ps1 -Jdk8Home F:\dev\jdk8-temurin

java --patch-module jdk.unsupported=.\iteration1\scripts\ceda-jdk17-shim\classes ...
```

源码必须用 **JDK 8 的 javac** 编译（JDK 9+ 不允许在已被模块占用的包里定义类）。

## 3. 还需要 legacy Netty 3（org.jboss.netty）

CEDA 服务端（com.adaptiveMQ3）依赖 `org.jboss.netty.channel.ChannelFactory`，而构建依赖里只有
Netty 4（`io.netty`）。启动时会抛 `NoClassDefFoundError: org/jboss/netty/channel/ChannelFactory`。
把 Netty 3 加到运行时 classpath 即可（二者包名不同，不会冲突）：

```
F:\dev\maven-repo2\io\netty\netty\3.7.0.Final\netty-3.7.0.Final.jar
```

## 4. 已知未决问题

完成上述三步后 QuoteSvr 能正常启动（并发布 HEARTBEAT），CEDA 客户端也会向
`10.130.21.53:20040 / 20041` 发起连接，但握手始终超时：

```
INFO  ats.app.ym.ceda.CedaClient - CedaClient connecting to 10.130.21.53:20041
WARN  110 LOW - ConnectionException: no reply
INFO: CEDA-api-4.0.1-SNAPSHOT Build: 20210330
```

即 TCP 可连、服务端不回包。已排除：注册节点缺失、RegType、NetType、本机未注册到注册中心
（以独立服务名注册后现象不变）。需要服务端侧信息才能继续定位（见 result.json 的 blockers）。
