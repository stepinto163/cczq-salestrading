using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Abstractions
{
    /// <summary>
    /// 行情中心域服务（数据接缝）。快照 + 推送的等价语义：
    /// 全量快照（Load*）+ 增量事件（SubscribeQuoteEvents）+ 断线轮询兜底（上层实现）。
    /// </summary>
    public interface IMarketCenterService
    {
        /// <summary>服务端语义的全部看板查询（Mock 阶段在进程内模拟）。</summary>
        Task<PagedResult<TapeRow>> QueryBoardRowsAsync(MarketBoardQuery query, CancellationToken ct);

        /// <summary>服务端语义的最优看板查询（Mock 阶段先筛选再聚合）。</summary>
        Task<PagedResult<BestRow>> QueryBestRowsAsync(MarketBoardQuery query, CancellationToken ct);

        /// <summary>加载"全部"看板行（原型：live_quotes 投影 board_credit_all/board_rate_all）。</summary>
        Task<IReadOnlyList<TapeRow>> LoadBoardRowsAsync(BondMarket market, CancellationToken ct);

        /// <summary>加载"最优"看板行（按 代码|价格类型|清算速度 聚合双向最优）。</summary>
        Task<IReadOnlyList<BestRow>> LoadBestRowsAsync(BondMarket market, CancellationToken ct);

        /// <summary>加载模糊报价（原型 0910 改名；接口仍为 GET /api/board-intents?start_date=...）。</summary>
        Task<IReadOnlyList<BoardIntentRow>> LoadIntentRowsAsync(DateTime start, DateTime end, CancellationToken ct);

        /// <summary>更新模糊报价状态。</summary>
        Task SetIntentStatusAsync(IReadOnlyCollection<string> intentIds, IntentStatus status, CancellationToken ct);

        /// <summary>加载报价提醒（原型：board_alert 投影）。</summary>

        /// <summary>带侧栏筛选条件的提醒查询（通知域 view.open 条件下推；null=不过滤）。
        /// page/pageSize 客户端分页（服务端视图补拉支撑深页）。</summary>
        Task<IReadOnlyList<AlertRow>> LoadAlertRowsAsync(Queries.ProductFilter filter, int page, int pageSize, CancellationToken ct);

        /// <summary>标记提醒已读（后端 ficc.notification.read；弹窗"查看"与提醒页工具栏上报）。</summary>
        Task MarkAlertReadAsync(string notificationId, CancellationToken ct);

        /// <summary>读取及保存账户提醒开关；保存必须携带读取到的版本。</summary>
        Task<AlertPreferences> LoadAlertPreferencesAsync(CancellationToken ct);
        Task<AlertPreferences> SaveAlertPreferencesAsync(AlertPreferences preferences, CancellationToken ct);
        event Action<AlertPreferences> AlertPreferencesChanged;

        /// <summary>最近一轮推送实际变更的报价 Id（全部页增量闪烁判定）。</summary>
        IReadOnlyList<string> SnapshotTouchedQuoteIds();

        /// <summary>最近一轮推送实际变更的券 Id（最优页增量闪烁判定）。</summary>
        IReadOnlyList<string> SnapshotTouchedSecurityIds();

        /// <summary>加载右侧债券详情（含多档行情与成交行情）。</summary>

        /// <summary>按代码查债券库（关注债券识别回填；未命中 Found=false）。</summary>
        Task<BondLookupEntry> LookupBondAsync(string code, CancellationToken ct);

        /// <summary>订阅报价变更事件（原型 SSE：new_quote/edit_quote/revoke_quote）。回调线程不定，UI 层需自行调度。</summary>
        void SubscribeQuoteEvents(Action<QuoteEvent> handler);

        /// <summary>退订报价变更事件。</summary>
        void UnsubscribeQuoteEvents(Action<QuoteEvent> handler);
    }
}
