# 原需求界面的 GW 接入版

本版直接使用已有需求界面的 HTML、CSS 和 app.js，保留原导航、行情表、右侧盘口、精确报价表、报价确认表、报价管理及弹窗。上一版重新绘制的简化界面已移除。

`requirements/` 中的 17 个原脚本和样式文件原样复制，哈希记录于 `source-manifest.json`。HTML 只调整资源路径、CSP、脚本接入，并移除应用 body 后的浏览器扩展残留。消息与数据转换集中在 `src/requirements-adapter.js` 和 `src/requirements-model.js`。

## 启动

PostgreSQL、ZooKeeper、QuoteSvr、NotificationSvr 与 GW 需先运行，参见 [启动指南](../docs/local-startup.zh-CN.md)。GW 已配置本机 WebSocket 端口 18601。

```powershell
# 仓库根目录；需要 Node.js 22 或以上，无运行时 npm 依赖。
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-web-client.ps1
```

打开 <http://127.0.0.1:8088>，刷新旧页面即可看到原需求界面。模拟账号 `trader1`、`trader2`，密码均为 `trader1`。

如果 GW 尚未加载 WebSocket 配置，执行 `stop-gateway.ps1`、`start-gateway.ps1`。停止静态服务器使用 `stop-web-client.ps1`。

## 首轮业务检查

1. 原登录框输入账号密码，通过 GW 登录。
2. 打开“报价录入 → 精确报价”，点击原“示例填充”按钮，示例为 `Bid 2.5000 250001.IB 1000 T+0`。点击识别后，在原表格中编辑字段。
3. 勾选记录，点击原确认按钮。本轮接入将其保存为**待确认报价**，随后在“报价确认”中完成确认。
4. 行情中心“利率债”中查看该报价。选中债券，右侧原多档行情表显示独立 Price 流的盘口。
5. 本人报价处于最优档时，“我的最优”提醒由 NotificationSvr 推送；提醒开关与已读动作同样调用服务端。
6. 报价管理支持撤出、恢复和删除；行情中心工具栏与右键Refer、OCO／打包、编辑和操作记录均通过DPS调用QuoteSvr。编辑弹窗先读取完整报价并保留该版本，保存时若发生并发修改，提示版本冲突并保留弹窗，不覆盖他人修改。新建弹窗保存后仍需到“报价确认”完成确认。
7. 报价管理 → 历史报价：初次打开按默认前一业务日查询；查看今天的操作需将起止日期改为今天并点击“查询”。使用`ficc.quote.history.search`读取持久化的每日终态记录，没有实时订阅；每次200条，点击“加载更多”继续，当前界面上限1000条，超出请缩小日期范围。历史行的Unrefer／删除针对同一quoteId的当前状态，操作前重新读取版本，由服务器校验状态和权限。

机构、联系人输入框已接入 CRM：输入名称或代码，从原下拉提示中选定机构，再查询联系人；选项包含 ID，用于区分同名记录。手工名称可先保存为待确认报价，确认前须匹配 ID。配置与样例见 [CRM PostgreSQL 接入](../docs/crm-postgresql.zh-CN.md)。

登录不再调用无条件的 `ficc.base.query(kind=SECURITY)` 扫描全市场。已显示报价、行情和提醒涉及的债券，通过 `ficc.base.get(kind=SECURITY, ids=[...])` 在后台按需读取，每次最多 20 个代码。加载失败提示资料错误并暂显示代码，不退出登录，也不在每次渲染时反复重试；重新登录可重新加载。精确报价按用户输入的代码查询基础资料后再生成待提交记录，查询失败或代码不存在时明确报错。

## 接入边界

业务流为浏览器 → GW WebSocket/DPS → QuoteSvr / NotificationSvr。HTTP :8088 仅提供静态文件。Quote、Price、Notification 使用各自接口、主题和缓存，行情不再由浏览器从 Quote 集合计算；旧 SSE 和轮询入口停用。

`requirements-guard.js` 拦截尚未迁移的旧业务 HTTP 调用，返回“尚未接入”错误，不发送到原后台。剩余功能仍保留原界面，不能视为已完成接入：复杂条件筛选、关注分组维护、自然语言识别、价格换算、智能推荐等。历史日期查询已接入；历史工具栏条件目前只过滤已加载记录，不表示已支持全部服务端历史筛选条件。

首轮识别仅将严格固定格式文本填入原编辑表，使用原本地解析函数，不冒充 ParseSvr。每批最多 5 条。报价缓存最多 50 条本人报价、20 只债券、50 条提醒，右侧显示最多 50 档行；复杂条件筛选暂为只读，对价板债券搜索继续使用原控件。报价、行情和提醒已支持滚动触发 `view.query` 分页查询，仅缓存当前批次；滚动后进入稳定浏览模式，点击“有 XX 条变更”返回第一批并恢复实时更新。这不代表大数据量性能验收已通过。

推送缺号时停止应用增量，可用原行情刷新按钮重新同步；连接断开时重新登录。写请求超时不自动重发。控制台 `ficcMessageLog()` 可查看最近 500 条脱敏消息。

## 验证

```powershell
node --test iteration1/web-client/tests/*.test.mjs
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-web-client.ps1
```

原页面脚本/DOM 联调检查另使用 jsdom，仅安装到忽略的 runtime 目录，不随页面部署：

```powershell
npm.cmd install --prefix iteration1/runtime/dom-test jsdom@26.1.0 --no-audit --no-fund --ignore-scripts
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-requirements-dom.ps1
```

该检查加载实际原 HTML 与全部脚本，通过原页面处理函数新建、确认一条报价，检查原表格和右侧盘口内容，随后删除测试报价。Node 堆上限 160 MB，外部看门狗 35 秒，不测性能。结果记录于 `iteration1/runtime/requirements-dom-report.json`。

原脚本/DOM 联调已通过；浏览器视觉验收仍需手工进行。结果见 [测试记录](../docs/web-first-flow-test.zh-CN.md)。

2026-09-16操作回归增加：行情存在而QuoteView缓存缺失时的撤销、工具栏／右键动作、完整字段编辑、并发版本冲突、历史日期查询、历史查询失败提示及恢复、历史恢复操作、持久化操作日志。运行命令不变；只创建少量测试报价，结束时验证清理状态，不执行压力测试。
