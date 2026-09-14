# ID 生成与 STC 对齐

服务端 `ficc.common.IdGenerator` 改为调用 STC 既有 IdUtil，不再拼接随机 UUID。保留业务前缀，例如报价为 `Q-<Snowflake十进制字符串>`，关注分组为 `W-<Snowflake十进制字符串>`。JSON 与数据库继续使用字符串，避免前端 JavaScript Number 丢失精度。

## 复用路径

- DPS/GW：`ServerGwWrapper` 初始化 `com.app.common.utils.IdUtil`；销售交易服务调用同一个类。
- CEDA：`ServerCedaWrapper` 初始化 `sc.app.stc.utils.IdUtil`；销售交易服务按 `Consts.getNetType()` 选择对应类。

当前 DPS 路径使用完整 Snowflake 十进制值：41 位毫秒时间差、5 位 datacenterId、5 位 workerId、12 位毫秒内序号；纪元值 `1420041600000`。IdUtil 的 datacenterId 为 0。当前 STC 启动器根据注册实例序列名计算 `abs(hashCode % 31)` 设置 workerId。销售交易服务不另建 worker、不重复初始化。

运行库差异：本地 `sc-app-stc-3.3.0.jar` 的 CEDA IdUtil 对十进制结果执行 `substring(1, 19)`，与当前源码直接返回完整值不同。适配器保留所加载 IdUtil 的行为；业务代码不能依赖 ID 长度，也不能通过 ID 解析时间。当前服务使用 DPS 路径，不经过该截取逻辑。

此规则与当前 STC 框架一致，但序列名哈希取模不能保证不同实例的 workerId 永不重复；需要严格多实例唯一性时，应在公共框架层统一分配 workerId。时钟回拨仍按原实现抛错，不自动退回 UUID。

## 范围和兼容性

- 新报价、来源消息、分组、子分组、条件、流及游标等通过共享生成器取得新格式 ID。
- 已存数据库的旧 ID 不改写，引用和历史保持连续；新旧格式可共存，无表结构迁移。
- 通知 ID 仍按业务内容生成确定性哈希，事件 ID 仍为存储 epoch 加事件序号，保留原去重和顺序语义。
- C#、Web 的 requestId / clientQuoteId 保留客户端生成方式；它们用于关联请求，不替代服务端 quoteId。
- 模拟登录的 sessionId 仍使用随机 UUID，避免将凭证变成可预测的 Snowflake。

代码依据：`sc-app-stc/utils/IdUtil.java`、`SnowflakeIdWorker.java`、`ServerWrapper.java`，以及 `sc-app-common/com/gw/common/utils/ServerGwWrapper.java`。

## 本轮验证（2026-09-14）

Maven 全模块 `verify` 成功，共享库 10 项单元测试通过。本地服务重启后，经真实 GW 完成两个用户、两条报价的 27 项功能检查，覆盖新 ID 格式、确认、价格推送、通知、撤出与恢复及删除清理。实际生成示例：`Q-1549191779541057536`。未运行压力测试。
