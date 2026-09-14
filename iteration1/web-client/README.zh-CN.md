# 原需求界面的 GW 接入版

本版直接使用已有需求原型的 HTML、CSS 和 app.js，保留原导航、行情表、右侧盘口、精确报价表、报价确认表、报价管理及弹窗。上一版重新绘制的简化界面已移除。

`requirements/` 中的 17 个原脚本和样式文件原样复制，哈希记录于 `source-manifest.json`。HTML 只调整资源路径、CSP、脚本接入，并移除应用 body 后的浏览器扩展残留。消息与数据转换集中在 `src/requirements-adapter.js` 和 `src/requirements-model.js`。

## 启动

PostgreSQL、ZooKeeper、QuoteSvr、NotificationSvr 与 GW 需先运行，参见 [启动指南](../docs/local-startup.zh-CN.md)。GW 已配置本机 WebSocket 端口 18601。

```powershell
# 仓库根目录；需要 Node.js 22 或以上，无运行时 npm 依赖。
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/start-web-prototype.ps1
```

打开 <http://127.0.0.1:8088>，刷新旧页面即可看到原需求界面。模拟账号 `demo`、`demo2`，密码均为 `demo`。

如果 GW 尚未加载 WebSocket 配置，执行 `stop-gateway.ps1`、`start-gateway.ps1`。停止静态服务器使用 `stop-web-prototype.ps1`。

## 首轮业务检查

1. 原登录框输入账号密码，通过 GW 登录。
2. 打开“报价录入 → 精确报价”，点击原“示例填充”按钮，示例为 `Bid 2.5000 250001.IB 1000 T+0`。点击识别后，在原表格中编辑字段。
3. 勾选记录，点击原确认按钮。本轮接入将其保存为**待确认报价**，随后在“报价确认”中完成确认。
4. 行情中心“利率债”中查看该报价。选中债券，右侧原多档行情表显示独立 Price 流的盘口。
5. 本人报价处于最优档时，“我的最优”提醒由 NotificationSvr 推送；提醒开关与已读动作同样调用服务端。
6. 报价管理支持撤出、恢复和删除；原报价编辑弹窗也接入 QuoteSvr。新建弹窗保存后仍需到“报价确认”完成确认。

机构、联系人名称可先保存；填写名称后，确认仍要求匹配 ID。本轮尚未接入 CRM 选择控件，不能把名称自动视为已匹配。

## 接入边界

业务流为浏览器 → GW WebSocket/DPS → QuoteSvr / NotificationSvr。HTTP :8088 仅提供静态文件。Quote、Price、Notification 使用各自接口、主题和缓存，行情不再由浏览器从 Quote 集合计算；旧 SSE 和轮询入口停用。

`requirements-guard.js` 拦截尚未迁移的旧业务 HTTP 调用，返回“尚未接入”错误，不发送到原后台。剩余功能仍保留原界面，不能视为已完成接入：复杂条件筛选、关注分组维护、历史查询、CRM、自然语言识别、价格换算、智能推荐等。

首轮识别仅将严格固定格式文本填入原编辑表，使用原本地解析函数，不冒充 ParseSvr。每批最多 5 条。报价缓存最多 50 条本人报价、20 只债券、50 条提醒，右侧显示最多 50 档行；复杂条件筛选暂为只读，对价板债券搜索继续使用原控件。尚未实现完整的服务端筛选与滚动加载，不能用于大数据量性能验收。

推送缺号时停止应用增量，可用原行情刷新按钮重新同步；连接断开时重新登录。写请求超时不自动重发。控制台 `ficcMessageLog()` 可查看最近 500 条脱敏消息。

## 验证

```powershell
node --test iteration1/web-client/tests/*.test.mjs
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-web-prototype.ps1
```

原页面脚本/DOM 联调检查另使用 jsdom，仅安装到忽略的 runtime 目录，不随页面部署：

```powershell
npm.cmd install --prefix iteration1/runtime/dom-test jsdom@26.1.0 --no-audit --no-fund --ignore-scripts
powershell -NoProfile -ExecutionPolicy Bypass -File iteration1/scripts/test-requirements-dom.ps1
```

该检查加载实际原 HTML 与全部脚本，通过原页面处理函数新建、确认一条报价，检查原表格和右侧盘口内容，随后删除测试报价。Node 堆上限 160 MB，外部看门狗 35 秒，不测性能。结果记录于 `iteration1/runtime/requirements-dom-report.json`。

原脚本/DOM 联调已通过；浏览器视觉验收仍需手工进行。结果见 [测试记录](../docs/web-first-flow-test.zh-CN.md)。
