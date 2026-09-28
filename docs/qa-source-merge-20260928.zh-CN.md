# QA 源码合并记录（2026-09-28）

来源：`C:\Users\loong\Downloads\cgws-ficc\cgws-ficc`。
目标：本仓库 `iteration1/cgws-ficc-saletrading-*`；本机 STC/FICC 环境位于同级 `../qa_env`。

## 已合并

- 新增 `cgws-ficc-saletrading-crmsvr`：统一提供 CRM 检索、按 ID 查询、STC 镜像和 ParserSvr 名称匹配接口。
- common 新增 `StcCrmClient`、`crm.mode=stc` 及相关测试。QuoteSvr、NotificationSvr 可通过 `SERVER.CRMSvr` 使用 CRM。
- QuoteSvr 合并 intake 认证配置错误的定位信息；不输出 token 内容。
- 合并 QA 部署说明、CRM 消费方配置和建表 SQL 打包文件。三个 SQL 与仓库 `iteration1/sql` 对应文件一致；本次没有执行迁移 SQL。

## 保留本机版本及修正

- 保留本机 BaseSvr 缓存与引用版本的两个回归测试，未接受 QA 中对它们的删除。
- 不覆盖本机 ATS_ROOT、地址、端口、数据库配置及已有未提交工作。
- CRMSvr 补齐 `NetRtOutputStream`，避免仅初始化资源而未启动 DPS 服务监听。
- 修正 CRMSvr 日志配置，避免 `additivity=false` 且未配置 appender 导致日志消失。
- 修正机构简称为空时被合并为同一条搜索结果的问题，增加空值、空字符串、空白字符串测试。
- 本机 `qa_env/build-parser-local.py` 改为读取本仓库 common 和 CRMSvr 源码，避免继续编译旧导出副本。

## 验证结果

- Java 8：common 的 CRM/BaseSvr 定向测试 17 项通过；CRM 26 项通过，1 项需要显式数据库配置的全量快照测试未执行。
- QuoteSvr、NotificationSvr 编译打包通过。
- 更新并重启本机 CRMSvr，经过本机 GW 验证 6 项：机构/联系人精确匹配、分别按 ID 获取、按机构筛选联系人、非法分页参数拒绝。
- 实际核对 `qa_env/ats_root/log/CRMSvr.console.log` 和 `CRMSvr.log`：均包含上述 6 次完整请求/回复及组件初始化日志。
- 本次没有启动 FIX、ParserSvr 或压力测试。ParserSvr 原有原生字典初始化问题尚未解决。
- 源文件备份及首轮验证记录：`iteration1/runtime/qa-merge-20260928/`。

## 后续构建统一决定

统一使用 Java 8、QA 的 STC 依赖和主 `pom.xml`；GW 使用现有 QA 可运行包，不再编译 GW。
先在本地 Maven 仓库导入原始依赖，多个开发环境需要共享时再部署 Maven 私服。

原始 `sumscope:mvn-root:1.0.0` 未找到；经用户授权重建最小本机父 POM，明确标记为本机替代文件，不能上传覆盖 QA 原件。
QA 的私有 JAR 按原坐标导入，公共依赖通过 Maven Central 获取；common 显式声明直接使用的 Fastjson2。
主 POM 已完成 parent/common 安装与四个服务打包，95 项测试通过、2 项按条件跳过，另有 15 项 PostgreSQL intake 检查通过。
原始私有依赖 POM 不完整，不能声称依赖图与 QA 完全一致。准备方法和限制见 [QA Maven 依赖准备](../iteration1/build/qa-maven/README.zh-CN.md)。

旧兼容 POM、基础库补丁和构建工具、独立 C# 原型及学习示例已退出活动源码树；API 调试器已迁入自身所需的 DPS 代码与 DLL，并完成编译及登录/日志验证。
工具拒绝永久删除命令，因此旧文件与编译产物保存在忽略目录 `iteration1/runtime/retired-20260928`，不进入 Git 提交。STC 客户端和现有 QA 环境保留。
