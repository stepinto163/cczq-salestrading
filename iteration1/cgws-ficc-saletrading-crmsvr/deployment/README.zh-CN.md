# CRMSvr 部署说明

CRMSvr 是 FICC 侧唯一的 CRM 出口：它直连 `ficc_ibor` 的 `crm_*` 表，启动时把机构与联系人加载为内存目录，
并通过 STC 总线向消费方提供查询与全量镜像。消费方不再持有 CRM 数据库连接，也不再知道 CRM 表结构。

## 提供的 topic

| topic | 方式 | 消费方 | 说明 |
| --- | --- | --- | --- |
| `stc.crm.search` | 请求应答 | QuoteSvr / NotificationSvr（FICC adapter） | 分页检索，字段与分页结构同原 PostgreSQL 直连实现 |
| `stc.crm.get` | 请求应答 | 同上 | 按 id 取单条 |
| `stc.crm.lookup` | 请求应答 | parsersvr | `{kind, name, id, match, PageSize}` → `{Code, totalSize, datas}`，行内字段为 STC 的 `TBrokerCrm*` 字段名 |
| `stc.crm.inst.query` | 全量镜像 + 请求应答 | basesvr / C# 客户端 | `BrokerCrmInstQueryResult` |
| `stc.crm.contact.query` | 全量镜像 + 请求应答 | 同上 | `BrokerCrmContactQueryResult` |
| `stc.crm.account.query` | 全量镜像 | 同上 | 当前为空，`crm_*` 没有账号来源 |

`stc.crm.search` / `stc.crm.get` 是 FICC adapter 的增量能力（与 `stc.base.search` 同类），
不改动 STC 原生协议；镜像 topic 的报文由 `MsgHelper.Serializer` 生成，与 `CRMSvrClient` 的反序列化一致。

## 部署步骤

1. 将 Jenkins QA 包解压至 `/opt/sumscope/stc`，保留 `CRMSvr` 下 javalib、control、config 的相对位置。
2. 包内 `config/crmsvr.properties` 通过 `crm.sourceName=FICC_CRM` 读取
   `$ATS_ROOT/control/DBPoolConfig.ini` 中的连接；该数据源应指向 `ficc_ibor`，CRM 表位于 `public`。
   数据库账号与密码由共享 DBPoolConfig.ini 管理，无需另建 local 配置或密码环境变量。
   `control/CRMSvr/resourceCfg.ini` 的 `crm.properties` 必须指向此包内文件。
3. 在 `$ATS_ROOT/control/ATSConfig.ini` 中配置 `SERVER.CRMSvr` 的注册中心与监听地址。
4. 消费方（QuoteSvr / NotificationSvr / ParserSvr）在共享 `ATSConfig.ini` 中保留或新增
   `SERVER.CRMSvr` 发现条目，端口与第 3 步一致；parsersvr 本身无需改代码。
5. 设置 Java 8，工作目录切换至解压后的服务根目录，设置 `ATS_ROOT` 后启动：

   ```bash
   export ATS_ROOT=/opt/sumscope
   bin/start ./control/CRMSvr.ini
   ```

6. 内存：目录约 40 万条机构、4000 条联系人；启动时约需 `-Xmx1g`。镜像 topic 会在订阅时把全量机构
   物化为 STC DTO（实测 5 秒、峰值仍在 1g 内），订阅方较多或数据量继续增长时按比例调大。

## 消费方接入（QuoteSvr / NotificationSvr）

`adapters.properties`：

```properties
crm.mode=stc
crm.serverKey=SERVER.CRMSvr
crm.requestTimeoutMillis=3000
```

回退：改回 `crm.mode=postgresql` + `crm.sourceName=FICC_CRM` + `crm.schema=public`，即消费方自己连库；
两种模式返回完全相同的字段与分页结构。

## parsersvr 相关约定

- `CRMSvrClient` 按 `Consts.InstTagList`（`BOND,NCD`）过滤机构、按 `Consts.ContactBizType`
  （`BNC,BCO,ABS,NCD`）过滤交易员。CRMSvr 用 `crm.stc.instTagList`（默认 `BOND`）和
  `crm.stc.contactBizType`（默认 `BNC`）填充 `taglist` / `biztype`，消费方构建变了要同步。
- `idbkey` / `crmid` / `id` 取 `crm_org.id`、`crm_contact.id`；`instid` 取联系人所属机构 id，
  一个联系人只出一镜像行，取 id 最小的机构。**上线前需确认解析出的 company/trader 与这套 id 同源**，
  否则 parsersvr 的机构/交易员识别会全部落空。
- 机构按非空简称去重：有联系人的行优先，否则取 id 最小者。简称为空时按机构 ID 保留，不合并无关机构。
- `stc.crm.lookup` 的 `match=exact` 先用机构简称比对、再退回机构全称，对应 parsersvr 解析出的公司名；
  `match` 为其它值时按检索文本包含匹配，同原 SQL 路径。
- 联系人检索文本为 id、姓名与本方交易员字段，与现有 SQL 路径一致；机构名不参与联系人检索。
- 增量推送（`stc.crm.inst.notify` / `contact.notify` / `account.notify` / `clear`）未实现：
  数据变更靠重启 CRMSvr 生效，与现有 CRM 全量镜像的刷新方式一致。
