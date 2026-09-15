# Codex 审核与下一步（2026-09-15）

## 本轮先解决什么

上一任务已取消并由本任务接续，不是否定已有基础资料调查。现在优先完成：**本机模拟依赖下 C# → GW → QuoteSvr / NotificationSvr 的只读链路**。它无需 STC 测试账号；先把这个确定链路打通，再讨论真实身份接入。

固定参考：旧分支 `work/codex-20260914-002` 的结果提交 `ce08ab98a20c7b3906abf80c43180122698d1786`；其 `testedCommit` 为 `89539ee1624226a9fd4729dc01c92088f60f4f0e`。按路径读取旧 result.json 及必要代码，不遍历旧分支全部文件，也不整支合并。

## 我这边已经验证的基线

代码基线 `dd9195222f0a73a36ab4773b3c3d7734d1db592d`。2026-09-15 08:25（北京时间）使用已有本地构建产物启动 PostgreSQL、ZK、QuoteSvr、NotificationSvr、GW 后，通过真实 C# DPS DLL 执行 `ServiceDiscoveryProbe`：3项通过、0条报价创建。此证据是**本机模拟身份及基础资料**，不是 STC QA 登录。

- GW `NetType=gw`；本机 QuoteSvr、NotificationSvr 及 GW 共用本机 ZK。
- 模拟模式中 `SERVER.LoginSvr.ServiceName=QuoteSvr`，登录由 QuoteSvr 的模拟 handler 返回。真实集成模式才使用 QA 上的 `ValidationServer`。不能混用这两个身份路径。
- GW 的 Spring 配置使用 `RequestService=LoginSvr,QuoteSvr,NotificationSvr`；本机成功配置没有新增 `nodeFilePath`。
- `start-gateway.ps1` 将 `gw-local.jar` 放在基础库前，入口是 `GW`；请核对你那边实际加载的 GW 类、sc-app-stc/common/DPS 版本与 classpath 顺序，不假设 JAR 与手头源码同版。
- 源码 `CedaApi.initNodeClientWrapper` 对默认 `*` 节点从 requestService 取服务集，其他节点才依赖 nodeCfgs。所以看到 `servers:null` 不能直接认定“所有请求均因缺少路由表被丢弃”；必须确认所加载版本的具体请求路径。
- 旧分支把 GW `NetType=gw` 改成 `dps`。这只是需要对比的差异，不是已证明的根因；手头源码对非 ceda 值走 GW wrapper，旧运行库可能不同。

## 执行顺序

1. **先整理依赖与配置事实。** 从新 main 建工作分支，保留旧分支。记录 Java/DPS/GW/common/STC 版本、关键类的 CodeSource、实际工作目录、ATS_ROOT 及加载配置路径；不打印密码和会话。沿用已有内网 Maven 依赖工作，但仅迁入必要修复，确保GW仍参与默认构建。
2. **复现单注册中心的模拟基线。** 使用任务专属本机配置，QuoteSvr、NotificationSvr及GW均消费/注册本机ZK。不要夹带真实Base/Login配置，也不要依赖残留持久种子节点。若端口被用户服务占用，使用明确配置的空闲本机端口，不停止用户服务。
3. **用真实 C# 客户端验证。** 构建 `iteration1/client/QuotePrototype.csproj`，使用默认输出 `client/bin/Debug/net472/`，不要使用启动文档里较旧的 Discovery 输出。`gateway-api.dll` 在 Git 中不携带；若缺失，可从用户授权且版本匹配的 STC 客户端部署目录取得，留在本机，不提交DLL。无合法来源时明确报告，Java探针可用于定位，但不能替代C#验收。
4. **定位失败跳点。** 最多一次登录和三项查询，追踪客户端发送→GW解码→会话/登录处理→wrapper选路与onlineStatus→QuoteSvr handler→回包。记录时间、主题、脱敏请求标识、耗时/返回码与对应日志位置。打开有界DEBUG仅覆盖这次实验；看到 null 时先区分“超时无响应”和“业务失败响应”，不要猜测或绕过校验。检查登录请求是否使用SDK正常connect握手，而不是在未登录会话上随意发送普通requestSync。
5. **单独修正基础资料映射。** 见下节。完成后代码先提交，再对同一SHA执行Maven/C#检查，最后写结果。真实STC凭据缺失只在结果中列为后续事项，不再反复等待或追问C#构建许可。

默认端口下的只读探针：

```powershell
dotnet build iteration1/client/QuotePrototype.csproj -m:1
# 使用 Start-Process -WindowStyle Hidden 启动下面参数，并用 WaitForExit(20000) 外部看门狗保护。
.\iteration1\client\bin\Debug\net472\Ficc.QuotePrototype.exe --probe-services <本机结果文件>
```

如本机端口冲突，可给探针新增可选 host/port 参数，保留默认行为。不要改成无期限等待。全部收发继续使用现有MessageLog。

## 审核结论，不需要重新问业务

- **债项与主体不能回退替代。** `bondRating = issueRating`；`issuerRating = issuerRating`。缺失债项但主体为AAA时，bondRating必须仍为空；二者AA+/AA时分别保留。`bondRatingSource=ISSUER`并不能解决错误筛选语义。既有依据见 `docs/models/core-data-model-er.zh-CN.md` 对两字段的定义。
- `Datas`/`datas` 兼容有真实联调依据，可选择性迁入。中债“行权|到期”的拆分也有价值，但保留空值含义，需对单值/双值/缺段测试；不要在本任务扩大数据模型或改变业务估值口径。
- 静态 `ZkCluster.zk` 根因与本地源码一致，但只在**同一JVM需要不同ZK连接**时构成该问题。GW本来就是独立进程；新增进程本身不是修复。持久注册节点只证明发现可行，无法随服务退出失效，不作为正式方案。
- 旧分支默认构建跳过gateway、导入整个`gw/`目录，不接受直接迁入。若内网依赖需要发布私服，给出坐标及可复现方案；本任务不要求擅自发布外部制品。
- IBOR OpenAPI、登录审计不是STC LoginSvr/BaseSvr的请求证据，本任务不继续这条支线。SSH不可操作时报告即可，不读取私钥或尝试绕过堡垒机。

## 回复要短且可执行

result.json：本轮testedCommit、模拟链路每项结果、实际GW断点及证据、Maven各模块/C#构建结果、清理信息、最小剩余阻塞。历史结果仅标“沿用，未本轮复测”。详细补充最多一个短 notes.md，避免每轮复制整段调查史。完成或受阻后推送新分支，等待Codex审核，不合并main。
