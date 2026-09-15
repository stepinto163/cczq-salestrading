# C# 客户端运行目录

需要 Windows x64 和 .NET Framework 4.7.2 或更高兼容版本，无需 Visual Studio。

复制整个 net472 文件夹，运行 Ficc.SalesTradingClient.exe。所有应用 DLL、原生 libuv.dll 和运行配置均已包含；api-debugger 子目录供客户端的 API 调试器按钮使用，也可以直接启动其中的 Ficc.DpsApiDebugger.exe。

顶部填写目标 GW 地址、端口和业务账号。自动测试在“操作结果 / 开发工具 → 自动测试”内。

2026-09-15：从空输出目录构建，验证两个应用窗口初始化及原生 libuv 加载成功；没有连接服务或发送报价。此验证不替代目标 QA 环境的联网测试。