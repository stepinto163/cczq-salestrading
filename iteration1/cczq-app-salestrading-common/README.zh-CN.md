# 销售交易公共库

公共代码位于 `src/main/java/ficc/common/`：

- `client/QuoteSvrClient`：通过 STC `NetClientWrapper` 和逻辑服务键查询报价状态、订阅报价/价格及获取原生价格快照。
- `client/LoginSessionClient` / `SimulatorLoginSvrClient`：隔离本机模拟身份访问；接入真实 LoginSvr 时替换适配层。

- `JsonUtils`：JSON 构造、解析、序列化、深拷贝及确定性内容摘要。
- `ApiResponse`：STC 风格的成功／失败回复。
- `ApiException`：业务错误码及结构化错误详情。
- `RequestValidator`：请求必填字段及允许字段检查。
- `IdGenerator`：生成带前缀的 UUID 标识。
- `BusinessClock`：生成上海时区的业务时间字符串。
- `FilterEngine`：字段校验和筛选条件计算。
- `JdbcStore`：PostgreSQL 连接、事务、事件、幂等记录与设置表；不包含报价生命周期或通知规则。
- `ReferenceData`：第一轮固定账户及基础资料模拟器，后续接入 LoginSvrClient/BaseSvrClient 时替换。

产物：`target/cczq-app-salestrading-common.jar`。没有启动入口、Handler、Stream 或业务服务依赖。两个业务服务分别依赖此库，不互相依赖源码或 JAR。`lib/` 保存本地基础依赖种子，基础库构建脚本从这里读取。

在本目录执行 `powershell -NoProfile -ExecutionPolicy Bypass -File ../scripts/build-server.ps1 -Module common`。构建前停止两个业务进程；完整构建及启动见 [总说明](../README.zh-CN.md)。
