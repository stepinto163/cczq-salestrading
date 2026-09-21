# 基础库本地构建及兼容记录

采用用户允许的“理解接口并兼容修改”方案，没有回退 `sc-app-common`。改动仅作用于 `runtime/foundation-build` 源码副本；生成差异在 `compat/patches`，补丁逻辑在 `compat/patch_foundations.py`。

| 组件 | 差异与处理 |
|---|---|
| DPS Java master | 添加 common/GW 所需 `IMessageByte`：false 表示消费，true 继续普通回调，异常回退普通回调；使用现有通知路径，不新建队列、不合并事件 |
| DPS Java master | 为已有 MessageWrapper bodyMap 增加访问器；在 TopicManager 原有增删及清理路径维护精确 topic → session 反向索引 `getTopics2()` |
| common / STC | 移除本地 BBox 不存在的 `Consts.isShowActiveThread` 诊断赋值；STC 的 `requestAsync` 对齐本地 YM 的 `requestAsyn` 签名 |
| GW | 两个通知访问点从 DTO 访问改为当前 DPS 返回的 Map；wire JSON 不变，GW 仍是独立进程 |
| 老 foundation / Java 17 | `sun.misc` Base64 改用 JDK Base64，按实际依赖调整 XStream 和 DBPool 调用 |
| Windows 编码 | QuoteSvr、GW 明确使用 `-Dfile.encoding=UTF-8`；已验证 C# 中文原文往返一致 |

停止 QuoteSvr、NotificationSvr 与 GW 后，在项目根目录依次执行：

```powershell
python iteration1/scripts/build-foundations.py dps-api-java
python iteration1/scripts/build-foundations.py ats-foundation-utilities
python iteration1/scripts/build-foundations.py ats-bbox-framework
python iteration1/scripts/build-foundations.py ats-app-ym-foundation
python iteration1/scripts/build-foundations.py sc-app-common
python iteration1/scripts/build-foundations.py sc-app-stc
python iteration1/scripts/build-foundations.py gw
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/build-server.ps1
```

`sc-app-stc` 构建所需网络、FIX、系统模型及少量工具源码，并非整个仓库；其他已有类型及没有源码的私有依赖使用旧项目缓存 JAR。公开依赖从 Maven Central 下载，构建记录保留源码 commit 与输出 JAR 校验值。

QuoteSvr 使用业务基础依赖；标准 GW 使用独立的 `GW/javalib`，不再编译网关兼容版本。C# 使用 `stc-client` 的真实 DPS DLL；该 DLL 已合并 Newtonsoft 类型，项目不重复引用另一份 Newtonsoft 程序集。

C# DLL 的 `subscribe()` 对非空错误回复也可能返回 true。平台额外检查 GW 的回复 `Code`，失败时移除本地注册；已通过另一用户私有 topic 拒绝用例验证。

许可证验证沿用原 SDK，未删除或绕过。
