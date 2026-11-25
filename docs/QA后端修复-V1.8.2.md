# V1.8.2 后端增量合并回归候选

前序已发送版本V1.8.1。本次覆盖尚未发送的NCD行情修复，以及三服务Jenkins旧依赖清理。不是全量覆盖包；不包含客户端、POM、凭据、数据库迁移或Core修改。

## 组件与合并

- Common/Quote：本地aa608148，基线e9f8c310。行情信用板块包括CREDIT+NCD，显式产品筛选继续求交；普通报价/历史/提醒分类不扩大。Common旧STC映射补ABS/NCD。
- BaseSvr：6d6a641，基线c7300ee（产品前序4ed96cc已在V1.8.1交付）。只合并市场分类及测试增量。用户正在QA处理含权/计算问题，本包不修改或回退含权逻辑；必须保留QA修复，不要整文件覆盖。
- Quote/Notification/QtIntake Jenkinsfile：f51ab11c，部署前停服成功再删除固定/opt/sumscope/stc/<服务>/javalib/*.jar；部署后恰好一个Common JAR才继续初始化及启动。配置、日志、持久数据不删。QtIntake本机保持停止。
- QA已回邮R5：Common c419c09、Core e9faadd、Quote acc66a3、Notification affcfe6、QtIntake4f66691；Base e05cd96/#7。按QA当前分支合并补丁，保留QA的0.1.1-SNAPSHOT依赖和用户后续修复。后端聚合仓路径iteration1/<模块>对应QA各服务仓根目录，分模块应用；文件列表及哈希见manifest。
- 本次不改sslib@main为0.0.3，因尚未核对新版共享库接口。若QA归档包内已带两个Common，新增门禁会阻止启动，需要同时检查共享流水线是否clean及打包暂存目录；本次不宣称已修复未取证的构建环节。

## 已验证与待验

- NCD：Common36、Quote14、Base25项选定测试通过。Ubuntu原脚本恢复后真实Login/Base通过；真实Base→GW→Quote：112690653.IB资料NCD，信用全部2/最优1、显式NCD2/CREDIT0/RATE0、普通CREDIT0，分页和更新推送通过。
- Jenkins：逐一提取三份Jenkinsfile命令，在隔离目录运行shell回归：停服失败不清理、清理旧JAR、保留data、缺失或重复Common拒绝启动、一个Common允许启动，全部通过。不是QA Jenkins真实部署验收。
- 用户负责QA UI验证；实际QA部署、NCD撤销/还原专项尚未验收。当前为合并回归候选，不是全部验收通过版本。后续只读探针曾遇LoginSvr离线，不能把较早服务成功结果冒充持续可用。

## QA核对

1. 合并前记录当前提交，优先保留本次含权修复；Common构建发布后再构建依赖服务，Base及Quote分类改动配套部署。
2. 检查归档包与部署目录：Common均应恰好一个版本；正常升级后0.1.0不残留，模拟重复包应明确部署失败，不启动服务。
3. 原先NCD报价必须在信用全部/最优出现，明确选择CREDIT时仍排除NCD；检查分页、刷新、新报价及撤销/还原。
4. 回邮合并提交、Jenkins构建号、部署版本及实际页面/API结果。含权问题的正式DM枚举与QA修复证据仍需另行回传确认。

尚未解决项继续登记：发行人筛选、Base源端增量优化、QA含权语义和计算一致性，不作为本包已修复功能。
