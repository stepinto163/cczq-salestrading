# 用户补充的 QA 环境信息

2026-09-14：用户明确授权在 Windows 本机启动本项目服务，连接 STC，并通过已有 SSH 窗口只读检查 STC 日志。本任务替代 001；旧任务未允许启动服务，现已取消。

用户提供的 ZooKeeper 配置：

```ini
REGISTER.ServerList=REGISTER.Svr1
REGISTER.Svr1.Name=REGISTER1
REGISTER.Svr1.Host=10.130.21.53:2181
```

用户已从 WorkBuddy PC 验证此端点可达；无需反复询问地址。既有模板使用 `REGISTER.Local`，实际本机配置需同步将相关消费者的 `RegisterServerList` 改为 `REGISTER.Svr1`，不要只替换 Host 留下错误的注册服务键引用。其余注册及服务名设置应依据现有 STC 配置核对。

下一步是发现 LoginSvr/BaseSvr 节点，核对实际返回的业务 IP/端口及 Windows 到这些端点的连接；不得把 ZooKeeper 的 2181 当成业务端口。

用户在远程 Windows 上已打开 SSH 客户端，连接 STC QA 主机 `10.130.21.53`。若 WorkBuddy 可控制该窗口，可只读检查相关服务配置与有界日志。不能假定具有桌面控制能力，也不要重新索取或提取 SSH 凭据。无法操作时返回具体、最少的人工只读检查步骤。

Codex 本机还有用户提供的 `CC网络拓扑图.pdf`，尚未核对全文；当前以上信息来自用户直接提供的配置和 SSH 截图。原始拓扑 PDF 未随任务上传，不把尚未读取的拓扑内容当成事实。
