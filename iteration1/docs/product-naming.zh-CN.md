# 产品命名更新

当前产品统一称为“FICC销售交易平台”。名称调整不改变报价协议和业务能力边界，模拟依赖继续明确标注“模拟”，测试工具继续标注“测试”。

## 当前入口

- 建表 SQL：`iteration1/sql/001-create-tables.sql`。
- 新安装 schema：`ficc_salestrading`，QuoteSvr、NotificationSvr共用。
- C#工程：`iteration1/client/SalesTradingClient.csproj`，程序：`Ficc.SalesTradingClient.exe`，代码命名空间：`Ficc.SalesTrading`。
- Web脚本：`start-web-client.ps1`、`stop-web-client.ps1`、`test-web-client.ps1`。
- 模拟账户：`trader1`、`trader2`，模拟密码均为 `trader1`；admin、search、intake使用同一模拟密码。用户ID仍是U-001/U-002等原值，既有报价归属不变。真实STC账户不受影响。

## 已有数据库

本轮没有自动迁移任何运行中数据库。已有本机/QA私有配置仍可显式使用旧schema；不要直接把db.schema改成新名后启动。

停止两个服务并备份数据库后，执行 `sql/003-rename-salestrading-schema.sql`，再将两个服务的 `db.schema` 都改为 `ficc_salestrading`。脚本只改schema名称，保留表、数据、序列、权限；新旧schema同时存在时拒绝执行，以免误选数据。外部SQL中写死的旧schema引用需同步更新。

新数据库只执行001。已有双schema数据库先执行003改报价schema名，再执行001补齐通知表，最后按QA数据库指南执行002复制旧通知数据。

旧名称只保留在迁移检测、历史交接记录及确需引用的外部历史源码中。原始需求附件和第三方JavaScript不做批量文字替换，JavaScript的prototype语言属性不属于产品命名。

## 构建及启动

```powershell
dotnet build iteration1/client/SalesTradingClient.csproj -m:1
dotnet build iteration1/api-debugger/DpsApiDebugger.csproj -m:1
Start-Process .\iteration1\client\bin\Debug\net472\Ficc.SalesTradingClient.exe
```

Java两套POM入口保持不变。客户端与服务端应一起更新后再使用新的模拟账户；已运行旧版本需正常停止后重新启动。私有部署配置和旧日志不批量改写。

## 本轮检查（2026-09-15）

兼容版Maven全部模块verify通过，三个部署ZIP的运行库及新SQL路径检查通过；C#客户端、API调试器编译通过；Web单元测试11项通过，原需求界面的真实GW功能检查11项通过（创建1条测试报价，结束时撤出并删除）。C#只读服务检查3项通过，65条内存夹具的网格功能检查10项通过。未运行压力测试。

`test-schema-naming.py` 使用本机数据库中随机命名的两个隔离schema，验证已有记录保留、改名脚本可重复执行、001可重复执行并创建14张业务表，最后完整回滚。正式数据schema未改名；本机服务重启后继续使用私有配置指定的既有schema。STC版完整编译仍需内网仓库验证。
