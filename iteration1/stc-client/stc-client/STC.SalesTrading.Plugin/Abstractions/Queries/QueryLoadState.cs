namespace STC.SalesTrading.Plugin.Abstractions.Queries
{
    /// <summary>查询生命周期，供页面统一呈现加载、空结果和失败状态。</summary>
    public enum QueryLoadState
    {
        Loading,
        Loaded,
        Empty,
        Error,
        Stale,
    }
}
