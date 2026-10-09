# 客户端应答到渲染路径剖析（2026-10-09）

## 结论

主要成本已定位到客户端的**协议原文/基线快照维护**和 **WPF Render 优先级工作**。不是 JSON 反序列化本身，也不是服务器查询等待占主导。本次只加入默认关闭的计时探针，未修改产品查询、协议语义或集合更新策略。

同样的 500 条报价、单客户端、组合条件，得到 40/60 条匹配结果。详细剖析轮点击筛选 p50 **342.23 ms**、p95 **404.77 ms**；自动筛选 p95 **743.00 ms**（包含 300 ms 防抖）。结果正确，**100 ms 性能目标仍未达到**。

## 实测阶段

下表为详细剖析轮点击路径 20 个有效样本；单位 ms，nearest-rank 分位数。

|阶段|p50|p95|说明|
|---|---:|---:|---|
|SDK view.open 请求|10.77|22.11|包含网络、服务端和 SDK 解码，不等于纯 matcher 时间|
|JSON 转 DTO|0.36|0.53|`ProtocolCodec.Decode` 的 `ToObject`|
|原文及基线快照维护|106.73|131.68|`ProtocolCodec.Attach`；在 UI 线程执行|
|DTO 再投影为 JSON|32.35|50.09|`ViewProtocolClient.OpenAsync` 的 `ToJson`|
|基础数据合并、展示行映射|1.37|4.42|`QueryTodayRowsAsync`；基础数据已缓存|
|Rows.Clear + 逐条 Add|23.22|30.46|包含同步集合/视图通知|
|每样本 Render 优先级操作耗时之和|128.06|145.41|WPF dispatcher 操作，包含整个窗口的相关工作|

这些阶段包含嵌套：例如总 decode 包含 ToObject 和 Attach，dispatcher Normal 操作包含协议处理；**不能相加各阶段 p95，也不能重复累计嵌套区间**。详细起止时间、线程号与统计见[原始结果](../tests/ui/benchmarks/results/20261009-client-profile-500.json)。

`render.dispatch-wait` p95 为 146.41 ms。并不是 UI 线程空闲等待了这么久：dispatcher 记录显示期间执行了较长的 Render 优先级操作。测试显式调用 `UpdateLayout` 时，调用本身 p95 只有 0.004 ms，因为此前的 WPF 工作已经完成。这不能解读为布局没有成本。

本次没有 CPU 调用栈采样，尚不能把全部 Render 优先级耗时精确分配给某个单元格模板、绑定或布局函数。`Rows.Clear/Add` 后发生重建是下一步应验证的方向，不能直接宣称某个模板已被证明是根因。

## 为什么协议转换这么贵

位置：`STC.SalesTrading.Plugin/Ficc/Protocol/ProtocolCodec.cs`。

1. `Decode` 先用 Newtonsoft 把 JSON 转成 DTO，这部分实测很小。
2. `Attach` 递归遍历所有模型属性，反复获取反射属性及 `JsonPropertyAttribute`。
3. 每层模型都深拷贝原文，并通过 `Snapshot` 建立基线。
4. `Snapshot → Tree → Encode` 会再次递归处理子模型；祖先快照重复经过子树的快照、合并和复制。
5. 读取完成后，`ViewProtocolClient.OpenAsync` 又调用 `ToJson`，再次走编码/比较/复制，随后页面才转换为展示行。

该实现是为了保留未知字段、区分未修改字段与显式 null，支持修改请求的语义。问题是**完整读取应答也支付了同样的编辑跟踪成本**。强类型接口本身并不要求这种开销。

## 其他路径

- `LivePage.OpenAsync` 每次切换筛选会关闭旧视图、重新打开 stream、订阅、打开新视图。它有额外顺序往返，但本轮不是最大开销。
- `QuoteMgmtViewModel.ApplyProductFilter` 同时查询 Today 和隐藏的 History。本轮 History 返回空集合，映射和集合更新几乎为零；只能说明这组数据下影响小，不能推广到有大量历史结果时。
- Today 的建议列表、计数属性、AfterQuery 不是主要成本。
- SDK 返回后恢复到 UI continuation 的等待较小；本轮主要是恢复之后执行的同步工作较重。

## 后续优化顺序

1. 保留强类型协议，把读取应答与编辑/补丁语义分开。读取应答避免递归基线快照；未知字段保留和显式 null 的契约测试必须继续通过。
2. 避免 `JSON → DTO → JSON → 展示行` 往返；直接从读取模型映射展示行。反射元数据缓存可以辅助，但不能代替去掉重复子树处理。
3. 优化集合替换和视图刷新，保持虚拟化、选择状态和实时更新正确；用相同用例验证 Render 耗时是否下降。
4. 再评估复用 stream、只刷新可见页及后台解码。仅把现有昂贵转换移到后台不会消除总工作量。

这里是建议顺序，本次没有实现以上优化，也没有承诺优化后一定低于 100 ms。

## 测量与复现

沿用[500 条组合筛选基准](filter-latency.zh-CN.md)的构建和执行命令，额外传 `-Profile`：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/measure-filter-latency.ps1 `
  -Profile `
  -StcConfig 'D:\workspace\greatewall\ficc\qa_env\stc-client\STC.exe.config' `
  -ReportDirectory 'D:\workspace\greatewall\ficc\ficc\iteration1\runtime\filter-latency\new-profile'
```

- `ClientPerformanceTrace.Sink` 默认 null。只有显式 profiling 测试启用，探针仅记录阶段名、单调时钟、线程号，写入内存；最终统一保存，不记录报价内容或凭证。
- `filter-latency.json` 增加 profile 起止时间。样本 traceStart 包含点击前设置条件的时间，不等于点击计时零点；业务延迟仍使用原 endToEndMs。
- WPF dispatcher 钩子只存在于测试 worker，结束时解绑。Normal/Render 等操作是墙钟耗时，不是 CPU 时间；包含其内部的阶段探针。
- 测试完成通知改为异步 continuation，避免 PropertyChanged 内联重入测试逻辑。它只改变测试观察方式；因此不要把两轮小幅差异解释成性能改进或退化。`render.next-callback` 包含回调触发后恢复测试 continuation 的时间，不是纯帧等待。
- 本次顺序执行两轮，均为 500 条、每种交互 20 次，第一次定位大阶段，第二次细分 codec 和 dispatcher。没有提高负载。两轮全部结果集核对通过，清理成功，剩余报价 ID 为空。
- 首轮粗粒度 profile 点击 p95 418.65 ms、自动 663.30 ms；详细轮为 404.77/743.00 ms。样本少、存在运行波动及探针开销，未单独量化探针开销，不用于高负载 SLA。
- 行列虚拟化开启；第二轮实际生成 19–20 个行容器，不是渲染 500 行。QuoteSvr 沿用此前 Oct8 本机运行包，没有部署数据库列迁移，也没有修改 QA 客户端。
- 终点仍是 WPF 布局后 Rendering 回调，不是屏幕像素实际呈现。没有测试 QA 网络、多用户、多债券冷缓存、行情中心。
- 外层 180 秒/768 MB watchdog 及 worker 内部超时保留；每次性能运行仍须用户授权，不自动加压。

完整运行日志在仓库根目录 `iteration1/runtime/filter-latency/20261009-profile/` 和 `20261009-profile-detail/`，未提交。提交的 JSON 保存详细轮原始阶段数据、41 个样本、阶段汇总和清理状态；汇总按样本时间窗关联事件，codec 子阶段按外层 view.open decode 时间窗归属。
