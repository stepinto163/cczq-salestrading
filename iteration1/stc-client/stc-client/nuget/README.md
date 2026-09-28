# nuget/ · 项目级离线包源

本目录是仓库唯一的 NuGet 包源，由根目录 `NuGet.config` 指定（同时 `<clear />` 掉了 nuget.org）。
Jenkins 构建机不需要外网即可 `-t:Restore`。

还原展开目录为 `packages\gpf`（`.gitignore` 第 21 行已忽略 `packages/`），不污染开发机全局缓存。

## 包清单

| 包 | 版本 | 引入原因 |
|---|---|---|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM 基础设施（`ObservableObject` / `[RelayCommand]` / `WeakReferenceMessenger`） |
| System.Memory | 4.6.0 | 上者传递依赖 |
| System.Buffers | 4.6.0 | `System.Memory` 传递依赖 |
| System.Numerics.Vectors | 4.6.0 | `System.Memory` 传递依赖 |
| System.Runtime.CompilerServices.Unsafe | 6.1.0 | 上两者传递依赖 |
| System.Threading.Tasks.Extensions | 4.5.4 | `Microsoft.Bcl.AsyncInterfaces` 传递依赖 |
| Microsoft.Bcl.AsyncInterfaces | 8.0.0 | CommunityToolkit.Mvvm 传递依赖 |
| System.ComponentModel.Annotations | 5.0.0 | CommunityToolkit.Mvvm 传递依赖（编译期，`ObservableValidator` 用；运行期不落盘） |

**注意**：`STC.Common.csproj` 的 `TrimMvvmFacades` target（见 `build/Mvvm.targets`）只把运行期必需的
程序集落到 `client\bin`。v4.7.2 实测白名单 6 个：
（`CommunityToolkit.Mvvm` / `System.Memory` / `System.Buffers` / `System.Numerics.Vectors` /
`System.Runtime.CompilerServices.Unsafe` / `Microsoft.Bcl.AsyncInterfaces`）。
其中 `Microsoft.Bcl.AsyncInterfaces` 在 net472 下由 NuGet 选中 net462 资产（4.6.1 时代走 netstandard2.0
会被裁剪），`AsyncRelayCommand` 的 `IAsyncDisposable` 运行期需要，故显式保留；
`System.Runtime.InteropServices.RuntimeInformation` 自 .NET Framework 4.7.1 起内置（mscorlib），不再落盘。

落盘的 6 个 DLL 必须与 `client/control/filedetailservice.xml` 的增量更新清单保持一致。

## 新增或升级包

```bash
# 在能联网的机器上拉全依赖链
nuget install <PackageId> -Version <Version> -OutputDirectory /tmp/pkgs -Source https://api.nuget.org/v3/index.json
# 把 .nupkg 拷进本目录, 提交, 并更新上表
find /tmp/pkgs -name '*.nupkg' -exec cp {} nuget/ \;
```

改完务必本地跑一次 `MSBuild STC.sln -t:Restore,Build -p:Configuration=Release`，
并核对 `git status --porcelain client/bin` 的新增文件数是否仍为预期值。
