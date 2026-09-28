# 最小项目上下文

更新时间：2026-09-28。此文件说明边界和已知状态，不代替源码或测试报告。

## 实现入口

- 主前端：iteration1/stc-client/stc-client；正式 STC 宿主中的 STC.SalesTrading.Plugin。已合入恢复的 LSJ 产品界面，不重新引入独立 C# 原型/学习示例。
- 后端：iteration1/cgws-ficc-saletrading-{parent-pom,common,quotesvr,notificationsvr,crmsvr,qtintakesvr}。
- 使用 STC/BBox/DPS 的 resource、handler、stream、control 模式。Java 8，主 pom.xml 面向 STC；不恢复 compat POM，不重写 GW。
- 报价与行情价格是不同模型、接口、推送频道。页面使用 view.open / view.query，不恢复 view.extend。
- QuoteSvr 管理报价和行情价格；NotificationSvr 依赖报价/行情产生提醒。不自行拆出单独 FICC MDSvr。

按任务需要再读 [API](../docs/ficc-api-spec.zh-CN.md)、[筛选字典](../docs/product-filter-contract.zh-CN.md)、[STC 开发原则](../docs/stc-development-principles.zh-CN.md)。架构中的规划不代表已实现。

## 构建与环境

仓库根目录 build.cmd / deploy.cmd，见[本机构建与部署](../iteration1/docs/local-build-deploy.zh-CN.md)。Java 用 Maven，客户端用 MSBuild。build 不停止进程；deploy 替换二进制并重启，只在任务明确授权时执行。

本机环境在仓库同级 qa_env/ats_root，这是机器本地目录，不保证另一环境存在。远端路径由操作者提供，不照抄本机绝对路径。真实服务、QA JAR、许可证、密码及数据库不随 Git 克隆自动准备完成。

本地重建 sumscope:mvn-root:1.0.0 仅供缺少原始 POM 的开发机，见 [Maven 依赖准备](../iteration1/build/qa-maven/README.zh-CN.md)。QA/Jenkins 使用原始构件，不向公司仓库覆盖同名根 POM。缺依赖先报告具体坐标，不新造兼容框架。

## 交接时的验证状态

- 2026-09-28 本机 build all 成功；Maven 报告合计 97 项测试无失败/错误，10 项部署保护检查通过。QuoteSvr 和 STC client 已部署检查启动，不代表全业务回归完成。
- UI 测试管理窗口已实现用例增删改、执行、状态及报告；三个业务用例曾在本机逐例执行通过。**管理窗口增删改全流程尚未完成实际 UI 验证，不能写成已验收。**
- 入口：[UI 自动化](../iteration1/stc-client/stc-client/docs/ui-automation.zh-CN.md)。现有三例为生命周期、精确报价识别与确认、待确认推送。
- 四类提醒、多用户、完整筛选和浏览稳定性仍需系统补齐测试，不把计划当作覆盖。
- ParserSvr 原生初始化可能很慢；CalSvr 曾因缺少匹配 Windows JNI 阻塞，不在普通任务中伪造计算结果。

这些是背景。新任务必须重新记录基线与实际验证，不能把旧数字当作新证据。
