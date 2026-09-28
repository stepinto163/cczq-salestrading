using System;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 宿主行情订阅通道（封装 STC.Common 的 PriceHandler / LastExecutionProcesser / 债券主数据字典）。
    /// 事件统一在 UI 线程（Application Dispatcher）回调；回调对象为归一化的不可变模型，可直接更新绑定集合。
    /// 实现方负责：同代码幂等、切券先退后订、宿主未就绪时静默（IsLive = false 且事件不触发）。
    /// </summary>
    public interface IHostQuoteFeed : IDisposable
    {
        /// <summary>宿主会话是否可用（LocalEnv 用户信息非空）；不可用时订阅为空操作。</summary>
        bool IsLive { get; }

        /// <summary>
        /// 切换订阅标的（传 null/空 = 退订全部）。
        /// 首帧来自通道缓存回放，之后为实时快照推送。
        /// </summary>
        void Switch(string securityCode);

        /// <summary>多档盘口快照（Bid 按价降序、Ofr 按价升序，每侧已截断）。</summary>
        event Action<QuoteFeedLevel[]> Snapshot;

        /// <summary>最新成交（缓存命中即回调一次，之后随新成交推送）。</summary>
        event Action<QuoteFeedLastExec> LastExecution;

        /// <summary>债券主数据（订阅时查一次；主数据后台加载完成时会再回调一次）。</summary>
        event Action<QuoteFeedBondInfo> BondInfo;
    }
}
