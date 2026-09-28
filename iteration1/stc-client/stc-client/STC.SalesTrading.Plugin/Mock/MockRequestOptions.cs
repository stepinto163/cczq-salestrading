using System;

namespace STC.SalesTrading.Plugin.Mock
{
    /// <summary>开发期可注入的请求场景，用于演示延迟、失败和空结果。</summary>
    public sealed class MockRequestOptions
    {
        public TimeSpan Delay { get; set; } = TimeSpan.Zero;
        public bool FailNextRequest { get; set; }
        public bool ReturnEmptyResults { get; set; }
        public string FailureMessage { get; set; } = "模拟请求失败";
    }
}
