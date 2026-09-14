# Codex / WorkBuddy 文件交接协议（讨论稿 v1）

目的：两个环境通过同一个 GitHub 仓库交换小型任务与结果，不需要传递整段聊天记录或遍历全部文档。协议独立于传输方式；未来 API 可直接传递相同 JSON。

## 固定入口

```text
handoff/
  README.md                  协议，只在首次使用或协议修改后读取
  INDEX.md                   当前任务路径，由发起方维护
  templates/task.json        任务格式
  templates/result.json      回复格式
  tasks/<taskId>/task.json    任务，由发起方维护
  tasks/<taskId>/result.json  回复，由执行方维护
```

指定任务时直接读取该任务，不必读取所有任务。未指定任务时先读 INDEX，只打开分配给自己的任务及其 `readFiles`。根据实际依赖再扩大源码搜索；这个入口不能替代适用的仓库开发规则。任务完成后只从 INDEX 移除，不搬动历史文件。

第一版人工通知对方“仓库、分支、taskId、路径”即可。Git 提交文件不会自动唤醒另一个工具；本稿不假设 WorkBuddy 支持 API、自动读取 AGENTS.md 或后台轮询。

已配置定时自动化时，每轮 fetch 后从 `origin/main` 读取固定入口。本机 main 落后并不需要先 pull 或等待确认；工作区干净时，可按上述发布提交创建独立工作分支。固定入口不存在即结束，不必遍历全部文件树和远程分支来寻找替代入口。

## 消息格式

统一 UTF-8 JSON，字段名英文、说明文字中文。模板是格式示例，不是已发布任务。

| task.json 字段 | 含义 |
|---|---|
| `protocolVersion` / `taskId` | 版本固定为 1；ID 全仓库唯一，如 `codex-20260914-001` |
| `from` / `to` | 发起与执行角色，`codex` 或 `workbuddy` |
| `state` | 发起方维护：`ready`、`accepted`、`cancelled` |
| `baseCommit` | 完整 Git SHA，任务依据的代码版本 |
| `workBranch` | 此任务唯一工作分支，如 `work/codex-20260914-001` |
| `goal` | 一句话目标 |
| `readFiles` | 首先读取的仓库相对路径，不复制文档全文 |
| `writePaths` | 允许修改的路径范围；超出范围先在结果中提出 |
| `constraints` | 环境、操作及负载限制 |
| `acceptance` | 可核验的完成条件 |

| result.json 字段 | 含义 |
|---|---|
| `protocolVersion` / `taskId` / `from` | 与任务匹配，回复者必须是任务执行方 |
| `state` | 执行方维护：`running`、`blocked`、`completed` |
| `testedCommit` | 实际验证的完整代码 SHA；未验证为 null |
| `summary` | 简短结论 |
| `changedFiles` | 修改的源码或配置路径 |
| `checks` | 每项包含检查名称、pass/fail/not_run、简短证据 |
| `blockers` | 阻塞问题及需要谁提供什么；没有则为空数组 |
| `nextAction` | 给接收方的一句话下一步 |

`completed` 只表示执行方交付完成，发起方审核后才将任务设为 `accepted`。不知道的结果写 `not_run`，不得把模拟通过写成真实环境通过。暂不增加 requestId、batchId 或额外消息 ID；taskId 加 Git 提交即可定位一次交接。

## Git 交接流程

1. 发起方在 main 发布 task.json，并在 INDEX 加入任务路径和 workBranch。此时 state=ready，baseCommit 指向已存在的代码提交。任务内容在交接后保持不变；验收标准变化时取消旧任务并创建新 ID。
2. 执行方 fetch 后，从包含该任务的发布提交创建指定 workBranch。确认 baseCommit 是该提交的祖先；不要从未知旧版本开始，也不要覆盖本机未提交修改。
3. 执行方在独占工作分支上写 result.json（running），按需提交代码。遇到问题用 blocked 返回明确问题，不把完整日志塞进回复。
4. 执行方先提交代码，再验证该代码提交。随后单独提交 completed 的 result.json，testedCommit 填前一个被验证的代码 SHA，并推送工作分支。这样避免把结果文件自身的提交 SHA 写入自身。验证后如又修改代码，必须重新验证并更新 testedCommit。
5. 发起方读取 result.json 和代码差异。核对 testedCommit 后合并工作分支，设置 task.state=accepted，并从 INDEX 移除。合并若改动了被验证的代码或解决了代码冲突，原测试结果不能自动代表合并后的版本。

发起方写 task/INDEX，执行方写 result/代码；同一工作分支同一时间只由一方推进。相同任务重复通知时先读已有结果，不重复执行数据库写入、部署或建单。中断重启先核对实际环境与提交记录再继续。发现任务已 cancelled 或 accepted 时不再执行。

阻塞任务的补充事实可由发起方在任务目录增加短小的 `answer.md`，在下一次通知中明确路径；若补充内容改变范围或验收要求，则创建新任务。第一版不增加聊天消息队列。

## 两个环境如何分工

- Codex：公共接口、业务实现、本机模拟验证、审核内网返回的差异。
- WorkBuddy：真实 LoginSvr/BaseSvr/CRM、内网 Maven、集成验证；必要时在任务范围内修复适配器。
- 两边都以实际检查为证据，代码通过分支合并同步；不建立两套长期分叉的业务实现。

首个试运行建议：WorkBuddy 读取适配指南，在内网验证一个真实 SID 和一只真实债券的只读查询，返回字段差异与检查结果。先不要安排完整压力测试。

凭据、SID、客户资料、原始内网业务报文留在内网；结果只放必要的脱敏样例和短摘要。大型日志留在 QA/Jenkins，只提供可定位的引用，并注明另一环境是否可访问。

## 启动提示词

```text
请处理仓库 <repo> 的任务 <taskId>。
先同步并读取 main 上 handoff/README.md 和 handoff/tasks/<taskId>/task.json。
按协议使用任务工作分支，只读取相关上下文，结果写入同目录 result.json。
本次范围以任务和当前用户指令为准；不要自动执行其他任务。
```

Codex 可在根 AGENTS.md 中添加到本目录的简短入口指引；官方机制见 [AGENTS.md 文档](https://learn.chatgpt.com/docs/agent-configuration/agents-md)。WorkBuddy 的入口设置需按其实际能力配置；目前可直接使用上述提示词。本协议是项目约定，并非任一产品内置的跨工具通信协议。

未来若确认 API 可用，只替换“通知、领取、回传”的传输层，继续使用 taskId、baseCommit、testedCommit 与相同验收规则。GitHub 仍可保存代码和交接记录。
