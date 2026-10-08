# 完整消息参考采集

`MessageProbe.java` 调用实际 `Parsing-1.8.7-SNAPSHOT.jar` 的 `BondQuoteParser.parsingAll`，字典使用 240210.ib、240215.ib、240001.ib 三个合成条目；日期为 2026-09-29。仅测试工具通过反射跳过连接数据库、定时刷新和全库初始化的构造器。生产代码没有使用这种方式。

结果：`src/test/resources/legacy-messages.json`，共 25 个完整消息。16 个样本参与逐字段等价断言；另外的样本保留原始结果用于说明差异，不能将它们计为通过的等价测试。用户明确选择修正双边价格吞数量和分号报价丢失，相关期望在 `QuoteMessageParserTest` 独立验证。

采集环境是 Windows，并非 Linux SO。二进制 SHA-256：

- Parsing JAR：`0faf47d90cae856793c895523a3a42f6570ed718ece9b62b6e52e3da80ff24a9`
- ParseLib.dll：`d3df85c550e5ea388b1f112a1e1e2e6a9b73ed047d619b8336896f83e5403559`

参考采集使用独立 128 MB、1 CPU JVM 与 15 秒进程树看门狗。默认 Maven 测试只读固定参考，不加载 DLL、不连接数据库。
