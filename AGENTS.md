# 最高优先级开发交付规则：同步 QA

- 用户要求：所有开发工作必须同步给 QA，不能停留在本机完成。同步范围包括已授权的代码、配置、数据库变更、接口约定、依赖、合并说明、验证结果和已知问题。
- 每次交付必须从上次已发送并核实的交付基线开始，逐项核对所有相关仓库的 Git 差异、工作备忘和邮件回执，覆盖此前已完成但尚未交付的改动；不得仅打包最后一个问题，也不得自行缩小范围。
- 维护逐项 QA 同步台账，登记组件、提交、发布版本、包名/哈希及“本地完成、已发送、QA已合并、已部署、已验收”状态；每个状态须有对应证据。SMTP接受不等于QA合并或验收，待办一直保留到闭环。
- 发送前逐项核销本次包、此前已发送包和未交付事项。未包含项必须说明原因、后续动作；不得用邮件里的“不包含某组件”替代完成原有交付要求。用户明确暂停或排除的项目按其指示登记，不能静默遗漏。
- 对交付范围或是否应同步存在疑问时，先核对已有授权和记录；仍不确定就问用户，不得自行排除重要修改。用户要求“打包发QA”默认覆盖全部尚未交付的已授权相关改动，而不是只覆盖最近一条消息。
- 未完成验证或存在疑点也必须向QA同步真实情况；供排查/回归的候选源码必须标明未验范围，不能作为已验收版本。同步不绕过既有测试、版本命名、同事共享库及前后端职责边界，不包含凭据或本机临时文件。
- QA通过既有163邮件渠道接收源码和说明；本规则授权范围内完成同步，无需用户重复提醒。收到QA回邮后更新合并、部署、验收状态及差异；没有回邮不得假定完成。不得据此擅自部署生产环境。

# AI collaboration

- For an assigned collaboration task, start with `handoff/INDEX.md` and its task file. Read `handoff/README.md` on first use; do not scan all historical tasks.
- Codex owns task design and review; WorkBuddy implements bounded tasks on separate branches/checkouts. Task scope and the user's latest instructions govern work.
- Use the task's `sourceBranch`, not an assumed `main`. Read the task at the fetched publication commit before creating its work branch.
- Report actual validation and untested behavior. Do not treat a worker's completion report as review approval or permission to merge.
- Use commit author `adrian` and the repository's existing email; do not change global Git identity.
- 完成并验证当前任务后主动提交相关改动，不要求用户重复提醒；用户明确要求暂不提交时除外。不要混入无关改动，并准确报告提交与验证结果。
- 每项任务在 `docs/task-memos/YYYY-MM-DD.md` 留简短中文备忘：目的、改动、实际验证及待办；同日任务追加记录，区分代码完成、本机验证和 QA/桌面验收。
- 工作记录入口为 `docs/task-memos/INDEX.md`；开始相关任务时按索引读取必要记录，不扫描全部聊天或历史。具体记录格式见同目录 `README.md`。
- 每次代码、配置、文档或环境改动，都要在结束任务前记录原因、实际改动、仓库/分支/提交或来源路径、验证和待办；失败、回退、未完成事项也保留。记录随相关改动一起提交。
- 跨仓库任务必须在本仓库索引登记各处备忘；交付前对照日期范围内全部相关备忘、Git差异和最终源码，区分最终保留、已回滚、仅调查、仅本机操作，不能只凭聊天回忆生成变更清单。
- 后续决定替代早期结论时追加更正和依据，不把旧方案写成最终交付。对外清单可以按用户要求简化，内部追溯记录仍保留。
- GitHub工作档案使用同一仓库的 `codex/work-records` 文档分支；按 `docs/task-memos/README.md` 的流程同步。明确区分本地已保存、已提交、已推送，不夹带产品代码发布，不把凭据或原始敏感日志写入工作记录。
- When the user writes in English, briefly offer a more natural version when useful, then continue the task. Preserve code identifiers and Chinese business terms; avoid lengthy grammar explanations.

# 同事共享库协作

- 职责边界：本方负责后端，前端由同事负责。默认开发与源码交付仅包含后端；即使本地保留STC客户端用于联调和端到端验证，也不得将客户端/插件源码或前端补丁混入交付包。只有用户明确要求某次前端修改或交付时才例外；历史授权不视为以后每次打包前端的授权。

- 修改同事维护的共享库前，先说明差异、理由和兼容性影响，与用户确认范围；不得以本项目未使用为由删除公共API、测试或原有中文注释。
- 保留同事原始Git历史；无历史的导入先单独提交未经修改的原版，再逐项提交获准改动，不能把修改混入首次导入。回贡献在独立分支提案，不直接改同事主分支。
- 新增或修改的代码注释、警告和错误说明使用中文；保留协议标识、错误码和日志检索键，不进行无关的全库文案替换。
- BondSearch Core原版明确约定布尔缺失按false、market允许NCD：保留这两项契约，不向同事库回贡献改变这些规则的补丁，由库所有者自行决定是否调整。FICC集成版现有差异的回退或迁移另行确定范围，不擅自连带修改运行系统。
- 本轮BondSearch Core回贡献限定为最小性能优化：不改变业务逻辑，不删除或改变任何已定义公共接口，即使接口目前未使用或设计不理想；不夹带正确性修复、格式化或架构改造。以原版可观察行为和接口兼容性为验收基准。

# 部署配置规则

- 交付版本命名必须沿用统一递增的发布序列，不能用日期、R1或提交哈希替代发布版本号。每次打包先查交付台账、上封已发送邮件及QA回传基线，确认前序版本；未核实时不得猜测下一版本号。
- 新交付采用cgws-salestrading-backend-V<主版本>.<次版本>.<修订版本>-<YYYYMMDD>.zip；邮件主题、包名、manifest和合并说明的发布版本必须一致，日期仅作辅助。版本升级幅度根据变更范围确定，不在不同日期重新起算。
- 每包登记前序交付版本、适用基线、完整/增量范围、各组件仓库及分支/基线提交/目标提交、SHA256和实际发送/QA合并状态。完整组件源码不等于包含全部组件的累计发布，必须明确未随包提供的依赖。
- 已发送包与版本不可覆盖、复用或静默改名；纠正时保留原件、哈希和邮件回执，以新的唯一版本或明确更正记录关联原交付。对历史未编号包先建立映射，经确认后再补齐版本链，不能跳过遗漏的交付。

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

# 端到端验收原则

- 功能修复交付前必须验证真实端到端链路，不能以单元测试、模拟数据或局部组件断言替代。字段改动须核对真实服务响应、实际客户端落库/缓存和页面结果，并覆盖原报错场景。
- 测试不得预先手工补入本应由上游提供的字段，从而绕过缺失链路。保留请求、响应、缓存与界面之间可核对的脱敏证据。
- 环境或依赖阻塞时明确标记端到端未完成，说明阻塞和已验证范围；不得称为全部通过或已验收。不要将未完成验收的修复作为验证通过的版本交付。

# 日志原值约定

- 用户要求DPS/FIX诊断日志记录实际字段值和完整主题名。不得自行增加字段遮蔽、脱敏占位符、嵌套JSON改写或额外省略规则；修改日志内容策略须先与用户讨论。保留用户批准的主题字节上限、截断标记与正常心跳开关。工作备忘及远端文档仍不保存凭据或原始敏感日志。
