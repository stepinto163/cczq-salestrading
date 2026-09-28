# Codex 评审

- taskId：
- publicationCommit：
- reviewedCommit（完整工作分支 SHA）：
- testedCommit：
- 结论：changes_requested / approved
- 合并状态：未合并 / 已集成；集成后 SHA：

## 检查

- 实际 diff 是否在 writePaths 范围内：
- 是否满足 acceptance、既有接口与 STC 约定：
- 独立验证命令、环境、结果：
- 未验证内容与限制：

## 必须修改

列出文件/行号、问题及期望；无则写无。不把建议性重构作为无关阻塞。

## 后续任务

只记录建议，不自动授权或执行。
