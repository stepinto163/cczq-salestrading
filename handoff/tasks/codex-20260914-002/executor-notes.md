# WorkBuddy 执行备注（codex-20260914-002）

仅记录执行侧补充分，不改变 result.json 的 state 与验收结论。

## 2026-09-14 晚：推送通路已恢复

- 结果文件 result.json（提交 `3d5db0e`）此前因 GitHub 凭据 401 未能推送，现已随凭证更新到达 origin/main 之外的本工作分支。
- 顺带确认：当时 `git push` 长时间挂起且无输出的根因是「令牌返回 401 + git 与凭据管理器交互卡死」，网络本身可达。

## 给发起方的两个提示

1. **冻结依赖不可通过 Git 传递**：`.gitignore` 排除 `**/runtime/` 与 `*.jar`，导致
   `cczq.salestrading.compat:stc-foundation-runtime:2026.09.14-5303c3cd5da4`（123 个 JAR）无法跨环境复现。
   任何新环境都会在 `clean verify` 阶段报同样的依赖缺失。建议后续把兼容依赖发布到内网私服（`bootstrap-maven-foundations.py --deploy-url`），或明确改依赖 `sumscope.stc:sc-app-stc` 基础构件。
2. **QA 的 `/MDTService` 下没有 LoginSvr**：真实登录链路在服务端缺一环。若后续任务涉及登录验证，请先确认登录服务的实际寻址方式（是否复用 AdminSvr / ValidationServer，或改用固定地址），避免执行方空转。

BaseSvr 实测可达：`10.130.21.53:20041`（Windows 侧 TCP 7ms），可作为固定地址诊断的候选。
