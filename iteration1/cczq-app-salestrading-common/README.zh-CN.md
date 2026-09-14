# 销售交易公共库

公共代码位于 `src/main/java/ficc/common/`：

- `client/QuoteSvrClient`：通过 STC `NetClientWrapper` 和逻辑服务键查询报价状态、订阅报价/价格及获取原生价格快照。
- `client/LoginSessionClient`：统一会话访问；`StcLoginSessionClient` 复用已有 `LoginSvrClient` 源码，模拟模式使用独立实现。
- `client/AdapterFactory`：按配置选择 Login、Base、CRM 实现，并加载外部 CRM JAR。
- `client/CrmClient`：机构与联系人扩展契约，支持模拟、空实现及独立插件。

- `JsonUtils`：JSON 构造、解析、序列化、深拷贝及确定性内容摘要。
- `ApiResponse`：STC 风格的成功／失败回复。
- `ApiException`：业务错误码及结构化错误详情。
- `RequestValidator`：请求必填字段及允许字段检查。
- `IdGenerator`：生成带前缀的 UUID 标识。
- `BusinessClock`：生成上海时区的业务时间字符串。
- `FilterEngine`：字段校验和筛选条件计算。
- `JdbcStore`：PostgreSQL 连接、事务、事件、幂等记录与设置表；不包含报价生命周期或通知规则。
- `ReferenceData`：基础资料与 CRM 的统一访问入口；`BaseDataClient` 可切换模拟数据与真实 STC 查询缓存。配置与联调范围见 [适配指南](../docs/stc-adapters.zh-CN.md)。

产物：`target/cczq-app-salestrading-common.jar`。没有启动入口、Handler、Stream 或业务服务依赖。两个业务服务分别依赖此库，不互相依赖源码或 JAR。`lib/` 保存本地基础依赖种子，基础库构建脚本从这里读取。

在本目录执行 `powershell -NoProfile -ExecutionPolicy Bypass -File ../scripts/build-server.ps1 -Module common`。构建前停止两个业务进程；完整构建及启动见 [总说明](../README.zh-CN.md)。
