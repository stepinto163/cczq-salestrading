# CRM PostgreSQL 接入

本文描述 `crm.mode=postgresql` 的直连方式。使用 `crm.mode=stc` 时，由[CRMSvr](../cgws-ficc-saletrading-crmsvr/deployment/README.zh-CN.md)访问同一 CRM 数据库，QuoteSvr 通过 DPS 查询；前端的 `ficc.base.query/get` 接口不变。

直连模式下，QuoteSvr 通过 `PostgresCrmClient` 只读访问独立 CRM 数据库；前端仍经 GW 调用 QuoteSvr 的 DPS 接口。当前不使用 `owner_user_ids` 做用户归属过滤。CRM 数据不复制到报价库，确认时将所选机构、联系人 ID 和规范名称保存到报价及历史。

| 来源 | 对外字段 |
|---|---|
| `public.crm_org.id / org_name / org_short_name` | `institutionId / institutionName / institutionShortName` |
| `public.crm_contact.id / name / position_name` | `contactId / contactName / positionName` |
| `public.crm_contact_org_rel.contact_id / org_id` | 联系人的 `institutionIds` 数组 |

ID 均为字符串。只查询 `deleted=false` 的机构、联系人；所属机构采用 `deleted=false AND is_current=true` 的关系。一个联系人可属于多个机构，不按姓名去重。只有一个所属机构时，同时返回兼容字段 `institutionId`。暂不增加状态、归属、部门等业务过滤。

## 配置与本机数据

将 [配置模板](../config/crm.postgresql.example.properties) 中的 `crm.*` 合并到部署已有的 adapter properties 文件，保留原有 LoginSvr、BaseSvr 设置。也可以放入现有 `quote.properties` 指向的文件；adapter 文件中的同名项优先。两个服务都使用该配置加载方式。

```properties
crm.mode=postgresql
crm.jdbcUrl=jdbc:postgresql://<host>:<port>/ficc_ibor
crm.schema=public
crm.username=<CRM只读账号>
crm.passwordEnv=FICC_CRM_PASSWORD
```

可省略 `crm.passwordEnv`，在不入库的本地配置中设置 `crm.password`。账号只需三张 CRM 表的 SELECT 权限；服务不会建表或修改 CRM。启动时验证连接和表结构；错误日志包含 SQLState、错误原因，密码不写日志。

本机独立库为 `127.0.0.1:55432/ficc_ibor`，测试数据由以下脚本创建：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-postgres.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/setup-crm-local.ps1
```

生成 `iteration1/runtime/crm.local.properties`，将其中 `crm.*` 合并到本机现有配置后重启服务。本轮已完成本机配置和重启。DDL 与合成数据见 [crm-local.sql](../sql/fixtures/crm-local.sql)：3 个机构、4 个联系人、6 条关系，包含删除、离职、同名及多机构关系。没有导入提供的真实联系人样本。标准 PostgreSQL 不使用来源 DDL 的厂商 `checksum` 参数。

## DPS 接口示例

服务：`QuoteSvr`；类型：request/reply。沿用当前登录会话。

### 搜索机构：`ficc.base.query`

```json
{"requestId":"CRM-1","kind":"INSTITUTION","text":"测试乙证券","limit":50}
```

```json
{"Code":"0000","Msg":"成功","requestId":"CRM-1","Data":{"records":[{"institutionId":"9000000000000000002","institutionName":"测试乙证券股份有限公司","institutionShortName":"测试乙证券"}],"hasMore":false,"nextOffset":null}}
```

`text` 匹配 ID、机构全称、简称、拼音；联系人匹配 ID、姓名及 CFETS 交易员 ID/姓名。查询在数据库执行，每次最多 200 条，客户端使用 50 条。服务额外读取一条判断 `hasMore`，不执行全表计数，因此 CRM 搜索不返回 `totalCount`。需要后续结果时，保持其他条件不变，将 `nextOffset` 作为下一请求的 `offset`；这不是冻结快照，目录并发变更后应重新搜索。

### 查询机构联系人：`ficc.base.query`

```json
{"requestId":"CRM-2","kind":"CONTACT","institutionId":"9000000000000000002","text":"测试张明","limit":50}
```

```json
{"Code":"0000","Msg":"成功","requestId":"CRM-2","Data":{"records":[{"contactId":"c9000000000000000001","contactName":"测试张明","positionName":"交易员","institutionIds":["9000000000000000001","9000000000000000002"],"institutionId":null},{"contactId":"c9000000000000000002","contactName":"测试张明","positionName":"销售","institutionIds":["9000000000000000002"],"institutionId":"9000000000000000002"}],"hasMore":false,"nextOffset":null}}
```

### 按 ID 获取：`ficc.base.get`

```json
{"requestId":"CRM-3","kind":"INSTITUTION","ids":["9000000000000000002"]}
```

```json
{"Code":"0000","Msg":"成功","requestId":"CRM-3","Data":{"records":[{"institutionId":"9000000000000000002","institutionName":"测试乙证券股份有限公司","institutionShortName":"测试乙证券"}],"missingIds":[]}}
```

联系人使用 `kind=CONTACT` 和联系人 ID，返回与查询相同的联系人结构。每次最多 200 个 ID。CRM 查询、按 ID 获取不依赖 BaseSvr 就绪。

### 确认报价：`ficc.quote.confirm`

```json
{"requestId":"CRM-4","quotes":[{"quoteId":"<待确认报价ID>","expectedRevision":"1","institutionId":"9000000000000000002","contactId":"c9000000000000000002"}]}
```

```json
{"Code":"0000","Msg":"成功","requestId":"CRM-4","Data":{"results":[{"quoteId":"<待确认报价ID>","revision":"2","Code":"0000","Msg":"成功"}]}}
```

报价其他必填字段须已齐全。服务重新按 ID 查询 CRM，验证所选机构在联系人的当前机构集合中，并写入规范名称。未匹配的名称仍可先保存为待确认报价；确认及有效报价修改均不能绕过匹配。


## 前端约定

搜索返回候选 ID 和名称，用户选择后在确认请求中提交 ID；只录入名称的报价可以保存为待确认，有名称却未匹配 ID 时不能确认。联系人必须属于所选机构，同名记录按 ID 区分。当前桌面界面以 STC 客户端为准，不再使用旧原型的按钮或测试入口。

QA 大表检索延迟需在目标环境验证，本地夹具不能代表真实数据规模。日志格式见[组件日志](component-logging.zh-CN.md)，QA 日志路径见[QA 部署](jenkins-qa.zh-CN.md#qa-日志)。
