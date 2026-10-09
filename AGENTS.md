# AI collaboration

- For an assigned collaboration task, start with `handoff/INDEX.md` and its task file. Read `handoff/README.md` on first use; do not scan all historical tasks.
- Codex owns task design and review; WorkBuddy implements bounded tasks on separate branches/checkouts. Task scope and the user's latest instructions govern work.
- Use the task's `sourceBranch`, not an assumed `main`. Read the task at the fetched publication commit before creating its work branch.
- Report actual validation and untested behavior. Do not treat a worker's completion report as review approval or permission to merge.
- Use commit author `adrian` and the repository's existing email; do not change global Git identity.
- 完成并验证当前任务后主动提交相关改动，不要求用户重复提醒；用户明确要求暂不提交时除外。不要混入无关改动，并准确报告提交与验证结果。
- When the user writes in English, briefly offer a more natural version when useful, then continue the task. Preserve code identifiers and Chinese business terms; avoid lengthy grammar explanations.

# 部署配置规则

- FICC 的部署配置由 Jenkins 流水线根据仓库中的配置、模板和环境参数自动生成，交付包不应要求手动复制 example 文件、补占位值或创建缺失配置。
- 共享配置统一使用 `$ATS_ROOT/control`，由部署自动化管理；服务包中的引用必须与实际生成路径一致。
- 不要自行以安全为由排除必要配置、另设凭据文件或增加密码环境变量。规则的例外和变更须先与用户讨论，不得擅自实施。
- 验证渲染后的配置、必要资源和加载路径；区分配置验证、实际服务启动和端到端验证，不得将前者报告成后者。

# Performance testing

## 本机运行契约

- 每个 Java 服务工程必须支持从源码目录在 IntelliJ Run / Debug：提交完整的 control/config 和共享运行配置，不能只让 Jenkins 打包结果可运行。
- 本机共享连接配置和许可证统一引用已有 ATS_ROOT；配置变更同时核对源码启动和 Salt 打包路径，不能留不存在的文件引用或要求手工补 example。
- 验证时明确区分配置加载检查与实际服务启动；调试同名服务之前需避免与已运行实例发生端口、存储锁或 FIX 会话冲突。

The user had to hard-reboot after an earlier grid benchmark hung the laptop.
Treat performance experiments conservatively:
- Never launch a performance test without explicit user authorization.
- Start with short, minimal-load runs and increase pressure only after reviewing responsiveness and results with the user.
- Keep grid virtualization enabled. Do not offer disabled-virtualization stress cases by default.
- Distinguish cached quote count from rendered rows; 10,000 quotes does not mean 10,000 rendered rows.
- Run tests under a separate process watchdog that can terminate a hung UI.
- Avoid concurrent benchmark instances and never automatically escalate load.
- Discuss in English; write documentation in Chinese.

# Naming and readability

- Use `cgws` for new project names and Maven coordinates (for example, `cgws-app-salestrading-quotesvr` and `cgws.salestrading`). Do not introduce new `cczq` names. Existing names should change only as part of an explicit migration.

- Treat this repository as the sales-trading platform. Use product terminology in current code, UI, filenames, scripts and documentation. Preserve literal names only where needed for historical evidence, external source references or backward-compatible data migration.

- Prefer clear, descriptive class and method names over brevity.
- Do not use cryptic abbreviations such as `J` for shared utilities.
- Use one statement per line, conventional indentation, and separate field declarations. Do not compress methods, branches, or several operations onto a single line.
- When changing existing code, format the affected methods clearly and use descriptive local variable names.
- Keep utility responsibilities explicit; separate JSON handling, API replies and validation.

# Startup and failure logging

- Every important component must log initialization with a consistent format: service, component, state, mode, implementation, target and elapsed time.
- Important startup messages must be visible at INFO in both the console and the startup log file. They must not depend on enabling DPS payload DEBUG logging.
- Configuration or initialization failures must log ERROR with the exception stack and propagate the failure. Do not swallow the exception, silently fall back to a simulator or report successful startup.
- Distinguish successful local initialization from remote connectivity and data readiness. Do not claim a remote dependency is ready merely because its client object was created.
- Preserve underlying causes and redact credentials from messages, connection addresses and exception stacks.
- Verify actual console/file output and failure propagation, not only the presence of logging calls in source.
