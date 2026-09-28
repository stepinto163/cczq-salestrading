# STC 服务发现

服务使用 STC 的标准启动、注册和客户端资源，通过 ZooKeeper 发现配置的目标服务，不在业务代码中写死远端 IP。机制依据见[STC 开发约定](../../docs/stc-development-principles.zh-CN.md)。

## 配置归属

- 注册中心地址及 `SERVER.*` 服务定义由对应环境的 `ATSConfig.ini` 维护。
- QA 使用共享 `$ATS_ROOT/control/ATSConfig.ini`；服务包不覆盖它，见[QA 部署](jenkins-qa.zh-CN.md)。
- 仓库内的启动脚本有独立本机配置，完整本机 STC 环境位于外部 `qa_env`；两套部署及脚本的范围见[启动说明](local-startup.zh-CN.md)。端口以各环境配置为准。
- `SERVER.LoginSvr` 必须指向当前选择的真实或模拟登录服务。模拟模式与 STC 模式的切换见[依赖适配](stc-adapters.zh-CN.md)。

## 内部订阅

NotificationSvr 通过 `QuoteSvrClient` / `NetClientResource` 接入 QuoteSvr，分别订阅报价事件与价格快照，再请求内部状态完成恢复。此链路不执行用户登录；前端业务请求仍须验证真实用户会话。

连接中断、代次变化或有界队列溢出会触发业务状态重建。连接恢复和资料就绪必须根据实际日志确认，不能仅凭客户端资源初始化成功判断。会话失效后，前端需重新登录并重新打开视图。

## 排障

1. 确认进程实际读取的 ATS_ROOT、工作目录及配置路径。
2. 检查注册中心可达、服务注册名与消费者配置一致，注册的地址可从调用方访问。
3. 比较进程日志与 ZooKeeper 节点；端口仍监听不代表临时注册节点仍存在。
4. 检查发现、连接、请求及恢复日志，日志位置见[组件日志](component-logging.zh-CN.md)和[DPS 日志](dps-logging.zh-CN.md)。

旧临时节点可能到会话超时后才消失；单次本机重连成功不代表长期中断、主备切换或双活已经验收。旧原型的服务发现测试脚本已移除，不再作为当前运行入口。
