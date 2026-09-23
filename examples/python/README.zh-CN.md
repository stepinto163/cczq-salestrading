# Python 请求与订阅最小示例

使用现有 `D:\workspace\idbpro-tool-server\gateway_api.py`，不修改或重写此库；只需要 Python 标准库。

在销售交易项目根目录运行：

```powershell
$env:PYTHONPATH = 'D:\workspace\idbpro-tool-server'
$env:FICC_USER = 'trader1'   # QA 使用真实 STC 账号
python examples/python/quote_subscribe.py
```

密码由终端隐藏输入。默认 GW 为 `127.0.0.1:18600`；可通过 `FICC_HOST`、`FICC_PORT` 改地址。TCP 客户端不要连接 WebSocket 端口18601。

流程：登录 → stream.open → subscribe → view.open（最多5条）→ 等待推送（最多12秒）→ view.close → disconnect。没有报价变化时会收到 HEARTBEAT。示例打印原始业务回包和推送，不创建/修改报价。它展示消息收发，不实现初始页与并发推送的缓存合并。

## 业务直连 QuoteSvr

```powershell
$env:FICC_DIRECT = '1'
$env:FICC_QUOTE_HOST = '127.0.0.1'
$env:FICC_QUOTE_PORT = '18680'
python examples/python/quote_subscribe.py
```

此模式先通过 GW 登录取得有效 sid，再建立携带 sid 的 QuoteSvr 直连；后续报价请求与订阅不经过 GW。它不是绕过身份认证，也不是完全取消登录路径。单纯把 GW 端口换成18680并发送账号密码，当前服务只完成传输连接，业务请求会因无有效会话被拒绝。

恢复全程经过 GW：`Remove-Item Env:FICC_DIRECT`。

## 已验证及库的注意点

- 2026-09-17，本机 GW18600 与 QuoteSvr18680 两条业务路径均验证：取得5条报价、订阅回包0000、收到 HEARTBEAT、关闭视图成功。未制造报价变更，因此不将此结果表述为业务 UPDATE 推送测试。
- QA 直连还要求服务地址可达、服务端能校验同一 STC 会话；本机结果不能代替 QA 验证。
- `subscribe()` 仅判断回复非空，错误 JSON 也可能返回 True，所以示例额外检查订阅确认 Code。
- 订阅内容应传原始 topic 字符串，不能再次 JSON 编码成带引号的字符串。
- 回调运行在接收线程，不要在回调里同步请求，否则接收线程无法处理回复。
- 库中 `unsubscribe()` 只删除本地回调。示例最终断开连接释放服务端订阅。
