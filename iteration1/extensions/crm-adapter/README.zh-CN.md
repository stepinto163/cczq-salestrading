# 独立 CRM 适配工程

这个 Maven 工程是内网团队的起点，可复制到独立仓库。当前实现返回空数据；业务服务通过 `CrmClient` 使用它，CRM 协议在此工程内实现。

## 接口约定

实现类需公开无参构造函数，并实现 `ficc.common.client.CrmClient`：

| 方法 | 作用 |
|---|---|
| `initialize(network, configuration)` | 接收 STC `NetClientWrapper` 及配置，可读取自定义 `crm.*` 项 |
| `institutions(userId)` | 返回此 STC 用户可选择的机构列表 |
| `contacts(userId)` | 返回此 STC 用户可选择的联系人列表 |
| `institution(userId, institutionId)` | 按 ID 校验机构；默认实现从列表查找，可覆盖为单条远程查询 |
| `contact(userId, contactId)` | 按 ID 校验联系人；默认实现从列表查找，可覆盖 |
| `close()` | 关闭适配器自有资源；共享网络资源由 STC 框架管理 |

示例返回值：

```json
[
  {"institutionId":"CRM-I-1001","institutionName":"示例银行"}
]
```

```json
[
  {"contactId":"CRM-C-2001","contactName":"张三","institutionId":"CRM-I-1001"}
]
```

列表无数据返回空列表；按 ID 不存在或用户无权访问返回 `null`。服务不可用应抛出错误，不伪装成成功或返回模拟数据。实现须允许并发调用，远程请求设置有界超时。

传入的 `userId` 来自验证后的 STC 会话；适配器负责落实 CRM 数据可见范围。联系人必须返回所属机构 ID，服务端会校验机构一致性，并使用 CRM 返回的标准名称保存报价。不要将第三方字段直接透传为业务模型。

## 构建与部署

JDK 17。先将同版本 `cgws-ficc-saletrading-common` 及其依赖发布到公司 Maven 私服，或在父工程执行 `mvn install` 安装到同一本地仓库。构建命令：

```powershell
mvn -B -ntp -f iteration1/extensions/crm-adapter/pom.xml package
```

产物：`target/cczq-salestrading-crm-adapter-0.1.0-SNAPSHOT.jar`。该工程不属于主服务 reactor，可独立交付。common 使用 `provided` 依赖，勿把 common、STC/DPS 基础库重复打入插件；插件额外依赖需自行打包进 JAR（必要时使用 shade 并隔离冲突依赖），当前加载器只加载配置指定的一个 JAR。

在服务适配配置中设置 `crm.mode=custom`、`crm.adapterClass` 和 `crm.adapterJar`，重启服务。详细示例见 [适配指南](../../docs/stc-adapters.zh-CN.md)。主业务工程保持不变；若未来修改 `CrmClient` 契约，需要同步升级插件版本并重新验证。
