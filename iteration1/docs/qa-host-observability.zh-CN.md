# QA 主机只读观测指南（STC `10.130.21.53`）

记录如何在**不使用 SSH、不依赖桌面窗口**的前提下，只读观测 QA 环境。
此前「读不到服务端日志」的结论已作废：障碍不是能力，而是**文件权限**。

> 所有命令均为只读。不要在此文件写入主机口令、bearer token 或库口令；
> token 由环境变量 `IDBPRO_EXECUTOR_TOKEN` 提供。

## 1. 主机与账号

| 项 | 值 |
|---|---|
| 主机名 | `GS-JYZX-1768552506773` |
| 系统 | Kylin Linux Advanced Server V10 (Lance) |
| 地址 | `10.130.21.53/22` (eth0) |
| 数据盘 | `/data`（400G，STC 运行目录） |
| 应用盘 | `/opt/sumscope`（JDK 与 javalib） |
| STC 运行用户 | `sumscope` (uid 1000) |

## 2. 通道一：executor agent（HTTP 控制面）

目标机上有一个标准库实现的 HTTP agent，**直接监听 8899**（不是反向隧道）：

```
http://10.130.21.53:8899         # 注意是 .53，不是 .51
GET  /health                     # 免鉴权
GET  /ops                        # 需 Authorization: Bearer <token>
POST /run                        # 执行已配置操作
POST /files/download|upload      # 文件传输
POST /tail/start|poll|stop       # 增量 tail，支持 filter（子串 grep）
```

systemd 单元：`idbpro-executor-agent.service`，以专用非特权用户 `executor` 运行。
当前仅启用 **4 个只读操作**：`identity`、`system_snapshot`、`journal_tail`、`service_status`（`shell` 未启用）。

客户端：`cczq-salestrading-automation/work/agent.py`（stdlib，token 走环境变量）。

### 三个必须知道的坑

1. **`os.path.isfile()` 把 EACCES 报成 `file does not exist`。**
   受限目录下的文件会被误报为「不存在」。
   实证：agent 称 `/opt/sumscope/stc/LoginSvr/javalib/*.jar` 不存在，
   但该 JAR 明确在运行中 LoginSvr 的 `java.class.path` 上。
   → **不要用「下载失败」推断文件不存在**，那只能说明当前用户没权限。
2. **目录无法下载**，因此 agent 不能列目录。
3. **`journal_tail` 对 STC 无效**：`journalctl` 以 `executor` 运行只能看到自己的 unit，
   查其他服务一律返回 `You are currently not seeing messages from other users`。

### 可读 / 不可读（实测）

- 可读：`/etc/*`、`/proc/<pid>/cmdline`、`/opt/sumscope/control/*`、`/opt/sumscope/tpc/jdk/*`、`/opt/idbpro-executor/*`
- **不可读**：`/data/stc/*`、`/opt/sumscope/stc/*`（STC 应用与日志目录）、
  `/proc/<pid>/maps|environ|fd/*|cwd|root`

日志不可读的**最小解法**（二选一，需有权限者执行）：

```bash
# (a) 给 executor 开只读 ACL
setfacl -R -m u:executor:rX /data/stc/LoginSvr/log

# (b) 让 agent 配置新增一个窄范围只读操作（例如 tail_log，路径白名单），再走 /admin/update
```

## 3. 通道二：JMX（信息量最大，且无需凭据）

每个 STC 服务都以如下参数启动：

```
-Dcom.sun.management.jmxremote.port=600xx
-Dcom.sun.management.jmxremote.authenticate=false
-Dcom.sun.management.jmxremote.ssl=false
-Djava.rmi.server.hostname=10.130.21.53
```

因此**本机可直接连接其平台 MBeanServer，无需任何凭据**。
连接串：`service:jmx:rmi:///jndi/rmi://10.130.21.53:600xx/jmxrmi`

实测服务 ↔ JMX 端口映射（`user.dir` 全部为 `/data/stc/<Service>`）：

| JMX | 服务 | JMX | 服务 |
|---|---|---|---|
| 60002 | BaseSvr | 60011 | MMSvr |
| 60003 | LoginSvr | 60012 | QuantSvr |
| 60004 | TBookSvr | 60013 | OHLCSvr |
| 60005 | MDSvr | 60014 | AnsSvr |
| 60006 | EMSSvr | 60019 | MDHistSvr |
| 60007 | INNERSvr | 60022 | QESvr |
| 60008 | RiskSvr | | |
| 60009 | CalSvr | | |
| 60010 | AdminSvr | | |

60000/60001 与 60015–60018、60020/60021 无监听；`10.100.5.40` 上的服务对本机不暴露 JMX。

工具（只调用 `getAttribute`，绝不写）：

- `cczq-salestrading-automation/work/JmxProbe.java` — 系统属性 + MBean 域统计
- `cczq-salestrading-automation/work/JmxBean.java` — 按名称子串 dump 只读属性（Druid / JDBC）

JMX 能拿到的关键事实（实测）：

- `user.dir=/data/stc/<Service>`、`sun.java.command=<Service>`、完整 `java.class.path`
- **Druid 数据源**：LoginSvr 指向 `<STC 库>`，用户 `ficc_user`
  → **与原型使用的是同一个库**，`ficc_demo` schema 与 STC 业务表同库。
- 运行时健康状况：连接池 `PoolingCount`/`ActiveCount`/`ErrorCount`、内存与线程。

## 4. 服务端配置核对（已实测）

`/opt/sumscope/control/ATSConfig.ini`（可读）给出全部服务注册信息，其中：

```ini
REGISTER.Svr1.Host = 10.130.21.53:2181
SERVER.LoginSvr.ServiceName = ValidationServer
SERVER.LoginSvr.Host = 10.130.21.53:20040
```

→ 与本项目 `iteration1/config/stc-consumers.example.ini` 的修正**一致**，
即 LoginSvr 以 `ValidationServer` 名称注册这一结论已在线上得到验证。
服务端口区间为 20040–20058（RDISvr/SYNCSvr/CDHSvr/QDMESPSvr/ODMSvr/CMDSSvr/SORSvr 在 `10.100.5.40`，
IPDSvr 在 `10.100.151.44:20047`）。

另注：`../../control` 从 `user.dir=/data/stc/<Service>` 解析为 **`/data/control`**，
与 `/opt/sumscope/control` 并存——存在两套 control 目录，改动配置时需确认生效的是哪一套。
