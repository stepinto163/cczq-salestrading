using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Abstractions.Queries;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using STC.SalesTrading.Plugin.Gateway.Contracts.Notification;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 行情中心域服务（IMarketCenterService 网关实现，批次 5）。
    /// "全部"页 = price 域视图 mode=FULL 展开（每个 NoMDEntries 一行）；"最优"页 = mode=BEST 聚合
    /// （Bid/Ofr 各取第一价格档含并列）；提醒页 = notification 域视图；识别回填 = ficc.base.query SECURITY。
    /// 模糊报价看板后端无此域（R9）明确不支持；筛选第一期客户端过滤（字段下推待 dictionary.query 联调后二期）。
    /// </summary>
    public sealed class GatewayMarketCenterService : IMarketCenterService, IDisposable
    {
        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<PriceStreamBatch> priceStream;

        /// <summary>已做过参考回填的券（ficc.base.get 只对新券发；服务生命周期 = tab 生命周期）。</summary>
        private readonly ReferenceDataCache referenceCache = new ReferenceDataCache();
        private readonly GatewayReportStream<NotificationStreamBatch> notificationStream;
        private readonly Dictionary<BondMarket, PriceLiveView> fullViews = new Dictionary<BondMarket, PriceLiveView>();
        private readonly Dictionary<BondMarket, PriceLiveView> bestViews = new Dictionary<BondMarket, PriceLiveView>();
        private readonly NotificationLiveView alertView;
        private readonly List<Action<QuoteEvent>> quoteEventHandlers = new List<Action<QuoteEvent>>();
        private readonly FrameCoalescer _quoteEventCoalescer;
        private readonly FrameCoalescer _alertCoalescer;
        private readonly object sync = new object();

        internal GatewayMarketCenterService(GatewayRequester requester)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            priceStream = new GatewayReportStream<PriceStreamBatch>(requester, GatewayTopics.PriceStreamOpen, StreamHandshake.Image);
            notificationStream = new GatewayReportStream<NotificationStreamBatch>(requester, GatewayTopics.NotificationStreamOpen);
            alertView = new NotificationLiveView(requester, notificationStream);
            // 提醒域推送 → 专用消息（提醒板块订阅静默刷新）；不走 QuoteEvent——
            // 行情板块订阅了 QuoteEvent，提醒域变化不该误触发行情重查
            // 两者均经同帧合并：一次后端变更会触发 full/best 两视图 Changed，同帧只广播一次
            _quoteEventCoalescer = new FrameCoalescer(() => PublishQuoteEvent(null));
            _alertCoalescer = new FrameCoalescer(() => STC.Common.Utils.Events.EventAggregator.Publish(
                new STC.SalesTrading.Plugin.Shared.Messages.AlertsChangedMessage()));
            alertView.Changed += _alertCoalescer.Signal;
            Action<string> onStreamFailed = reason => STC.Common.Utils.Events.EventAggregator.Publish(
                new STC.SalesTrading.Plugin.Shared.Messages.DataSourceResetMessage());
            priceStream.Failed += onStreamFailed;
            notificationStream.Failed += onStreamFailed;
        }

        /// <summary>推送信号同帧合并：netapi 回调线程置脏，一个 Dispatcher 帧内只冲刷一次。
        /// 先清调度标志再读脏标志——冲刷期间新到的信号会重新排队下一帧，不丢。</summary>
        private sealed class FrameCoalescer
        {
            private readonly Action _flush;
            private readonly System.Windows.Threading.Dispatcher _dispatcher;
            private int _scheduled;
            private int _pending;

            public FrameCoalescer(Action flush)
            {
                _flush = flush;
                _dispatcher = System.Windows.Application.Current?.Dispatcher;
            }

            public void Signal()
            {
                Interlocked.Exchange(ref _pending, 1);
                if (Interlocked.Exchange(ref _scheduled, 1) == 1) return;
                if (_dispatcher == null)
                {
                    Flush();
                    return;
                }
                _dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(Flush));
            }

            private void Flush()
            {
                Volatile.Write(ref _scheduled, 0);
                if (Interlocked.Exchange(ref _pending, 0) == 1) _flush();
            }
        }

        #region 全部报价页（mode=FULL 展开）

        public IReadOnlyList<string> SnapshotTouchedQuoteIds()
        {
            // 信用/利率各一个 view 实例——取并集（QuoteId 全局唯一，跨市场并集无误伤）
            lock (sync)
            {
                return fullViews.Values
                    .SelectMany(v => v.SnapshotTouchedQuoteIds())
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
        }

        public IReadOnlyList<string> SnapshotTouchedSecurityIds()
        {
            lock (sync)
            {
                return bestViews.Values
                    .SelectMany(v => v.SnapshotTouchedSecurityIds())
                    .Distinct(StringComparer.Ordinal)
                    .ToList();
            }
        }

        public Task<IReadOnlyList<TapeRow>> LoadBoardRowsAsync(BondMarket market, CancellationToken ct)
        {
            return QueryBoardRowsAsync(new MarketBoardQuery { Market = market, PageSize = 1000 }, ct)
                .ContinueWith(t => (IReadOnlyList<TapeRow>)t.Result.Items, ct);
        }

        public async Task<PagedResult<TapeRow>> QueryBoardRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            // 方向开关已下推 side 条件；两端全关为防御态（原型 toggle 至少一边），直接空页
            if (!query.IncludeBid && !query.IncludeOfr)
            {
                return Paginate(new List<TapeRow>(), query, 0);
            }
            var view = await GetView(fullViews, query, "FULL", ct).ConfigureAwait(false);
            var books = await EnsureRangeAsync(view, query.Page, query.PageSize, ct).ConfigureAwait(false);
            var userNames = UserNameMap.Build();
            var rows = books.SelectMany(b => ExpandBook(b, query.Market, UserNameMap.CurrentUserId, userNames)).ToList();
            // 服务端价格簿已按券、方向、可比收益率、时间排序，展开时保留顺序。
            await GatewayReferenceFill.EnsureTapeAsync(requester, rows, ct, referenceCache).ConfigureAwait(false);
            return Paginate(rows, query, view.TotalCount);
        }

        /// <summary>一券价格簿 → 每条目一行（TapeRow）。旗标条目不携带恒 false（批次 1 建模注释）。</summary>
        private static IEnumerable<TapeRow> ExpandBook(PriceBookRow book, BondMarket market, string userId, Dictionary<string, string> userNames)
        {
            if (book == null || book.SecurityId == null) yield break;
            var entries = book.MarketData != null ? book.MarketData.NoMDEntries : null;
            if (entries == null) yield break;

            foreach (var entry in entries)
            {
                if (entry == null) continue;
                var refs = book.QuoteRefs;
                QuoteRef quoteRef = null;
                var hasRef = entry.MDEntryID != null && refs != null && refs.TryGetValue(entry.MDEntryID, out quoteRef);

                yield return new TapeRow
                {
                    Market = market,
                    // MDEntryID 建议为 "FICC:quoteId"；关联报价身份以 quoteRefs 为准
                    QuoteId = hasRef ? quoteRef.QuoteId : (entry.MDEntryID != null && entry.MDEntryID.StartsWith("FICC:", StringComparison.Ordinal) ? entry.MDEntryID.Substring(5) : entry.MDEntryID),
                    Revision = hasRef ? quoteRef.QuoteRevision : null,
                    SecurityId = book.SecurityId,
                    SecurityName = book.MarketData != null ? book.MarketData.Symbol : null,
                    Side = ParseEntrySide(entry.MDEntryType),
                    Price = GatewayModelMapper.ParseDecimal(entry.MDEntryPx),
                    Quantity = GatewayModelMapper.ParseDecimal(entry.MDEntrySize),
                    SettlType = GatewayModelMapper.ParseSettlType(entry.SettlType),
                    // 决策 #3 已落地：价格簿条目随行下发机构/联系人（含 Id，新报价预填 CRM 选中值用）
                    InstitutionName = entry.InstitutionName,
                    InstitutionId = entry.InstitutionID,
                    ContactName = entry.TraderName,
                    ContactId = entry.TraderID,
                    OwnerUserId = entry.UserID,
                    // 决策 #2 兑现：显示名经用户映射（映射不到时保留主键）
                    OwnerUserName = Lookup(userNames, entry.UserID),
                    // 条目级报价时间（后端每条目带各自 updateTime 的日期/时间拆分值）；
                    // 簿级时间是该券全部报价的最大值，平摊给每行会造成"改一笔整券时间都变"（探针实证）
                    UpdateTime = ParseEntryTime(entry) ?? book.UpdateTime,
                    // 本人归属：条目 ownerUserId 与登录用户主键一致
                    IsMine = userId != null && entry.UserID == userId,
                    LastPrice = null,
                };
            }
        }

        /// <summary>条目级报价时间：MDEntryDate(yyyyMMdd) + MDEntryTime(HH:mm:ss.SSS)；解析失败回退簿级时间。</summary>
        private static DateTime? ParseEntryTime(MarketDataEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.MDEntryDate) || string.IsNullOrEmpty(entry.MDEntryTime)) return null;
            if (DateTime.TryParseExact(entry.MDEntryDate + " " + entry.MDEntryTime, "yyyyMMdd HH:mm:ss.fff",
                CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var time))
            {
                return time;
            }
            return null;
        }

        /// <summary>STC MDEntryType："0"=Bid / "1"=Ofr（与 FICC side 编码不同，勿混用 ParseSide）。</summary>
        private static QuoteDirection ParseEntrySide(string mdEntryType)
        {
            return mdEntryType == "1" ? QuoteDirection.Ofr : QuoteDirection.Bid;
        }

        #endregion

        #region 最优报价页（mode=BEST 聚合）

        public Task<IReadOnlyList<BestRow>> LoadBestRowsAsync(BondMarket market, CancellationToken ct)
        {
            return QueryBestRowsAsync(new MarketBoardQuery { Market = market, PageSize = 1000 }, ct)
                .ContinueWith(t => (IReadOnlyList<BestRow>)t.Result.Items, ct);
        }

        public async Task<PagedResult<BestRow>> QueryBestRowsAsync(MarketBoardQuery query, CancellationToken ct)
        {
            if (!query.IncludeBid && !query.IncludeOfr)
            {
                return Paginate(new List<BestRow>(), query, 0);
            }
            var view = await GetView(bestViews, query, "BEST", ct).ConfigureAwait(false);
            var books = await EnsureRangeAsync(view, query.Page, query.PageSize, ct).ConfigureAwait(false);
            var userNames = UserNameMap.Build();
            var rows = books.Select(b => AggregateBest(b, query.Market, UserNameMap.CurrentUserId, userNames)).ToList();
            await GatewayReferenceFill.EnsureBestAsync(requester, rows, ct, referenceCache).ConfigureAwait(false);
            return Paginate(rows, query, view.TotalCount);
        }

        /// <summary>
        /// 一券价格簿 → BestRow：Bid 取收益率最低档、Ofr 取最高档（MDEntryPx 为比较用收益率），
        /// 档内并列全部进 BidQuotes/OfrQuotes（同档浮层）。
        /// </summary>
        private static BestRow AggregateBest(PriceBookRow book, BondMarket market, string userId, Dictionary<string, string> userNames)
        {
            var row = new BestRow
            {
                Market = market,
                SecurityId = book != null ? book.SecurityId : null,
                SecurityName = book != null && book.MarketData != null ? book.MarketData.Symbol : null,
                UpdateTime = book != null ? book.UpdateTime : null,
            };
            if (book == null) return row;

            var entries = book.MarketData != null ? book.MarketData.NoMDEntries : null;
            if (entries == null || entries.Count == 0) return row;

            var bids = new List<BestEntry>();
            var ofrs = new List<BestEntry>();
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                var price = GatewayModelMapper.ParseDecimal(entry.MDEntryPx);
                if (!price.HasValue) continue;
                var item = new BestEntry
                {
                    Price = price.Value,
                    Quantity = GatewayModelMapper.ParseDecimal(entry.MDEntrySize),
                    OwnerUserId = entry.UserID,
                    Entry = entry,
                    UpdateTime = ParseEntryTime(entry),
                    QuoteId = ResolveQuoteId(book, entry),
                };
                if (ParseEntrySide(entry.MDEntryType) == QuoteDirection.Bid) bids.Add(item);
                else ofrs.Add(item);
            }

            // 收益率口径：Bid 低者优先、Ofr 高者优先（价格簿同价档合并规则）
            var bestBid = bids.Count > 0 ? bids.Min(e => e.Price) : (decimal?)null;
            var bestOfr = ofrs.Count > 0 ? ofrs.Max(e => e.Price) : (decimal?)null;

            var bidLevel = bestBid.HasValue ? bids.Where(e => e.Price == bestBid.Value).ToList() : new List<BestEntry>();
            var ofrLevel = bestOfr.HasValue ? ofrs.Where(e => e.Price == bestOfr.Value).ToList() : new List<BestEntry>();

            row.Bid = bestBid;
            row.BidVolume = bidLevel.Sum(e => e.Quantity ?? 0m);
            row.BidQuotesCount = bidLevel.Count;
            row.BidQuotes = BuildDetails(bidLevel, book, userId, userNames);
            row.Ofr = bestOfr;
            row.OfrVolume = ofrLevel.Sum(e => e.Quantity ?? 0m);
            row.OfrQuotesCount = ofrLevel.Count;
            row.OfrQuotes = BuildDetails(ofrLevel, book, userId, userNames);

            // 本人归属：任一侧最优为本人报价（v1 粗粒度；按右键单元格解析侧为二期）
            row.IsMine = userId != null
                && (bidLevel.Any(e => e.OwnerUserId == userId) || ofrLevel.Any(e => e.OwnerUserId == userId));

            if (book.QuoteRefs != null)
            {
                row.SourceQuoteIds = book.QuoteRefs.Values
                    .Where(r => r != null && r.QuoteId != null)
                    .Select(r => r.QuoteId)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }
            return row;
        }

        private static string Lookup(Dictionary<string, string> names, string userId)
        {
            string name;
            return userId != null && names != null && names.TryGetValue(userId, out name) ? name : userId;
        }

        private static string ResolveQuoteId(PriceBookRow book, MarketDataEntry entry)
        {
            QuoteRef quoteRef;
            if (entry.MDEntryID != null && book.QuoteRefs != null && book.QuoteRefs.TryGetValue(entry.MDEntryID, out quoteRef))
            {
                return quoteRef != null ? quoteRef.QuoteId : null;
            }
            return entry.MDEntryID != null && entry.MDEntryID.StartsWith("FICC:", StringComparison.Ordinal)
                ? entry.MDEntryID.Substring(5)
                : entry.MDEntryID;
        }

        private static List<BestQuoteDetail> BuildDetails(List<BestEntry> level, PriceBookRow book, string userId, Dictionary<string, string> userNames)
        {
            // 同价档按报价实际时间降序，展示每条报价自身的对手与归属人。
            return level
                .OrderByDescending(e => e.UpdateTime ?? DateTime.MinValue)
                .ThenBy(e => e.QuoteId, StringComparer.Ordinal)
                .Select(e => new BestQuoteDetail
                {
                    Price = (e.Price.ToString("0.0000", CultureInfo.InvariantCulture)) + (e.Entry.ExerciseFlag == "0" ? " 行权" : " YTM"),
                    Yield = (e.Price.ToString("0.0000", CultureInfo.InvariantCulture)) + "%",
                    Volume = e.Quantity,
                    Institution = e.Entry.InstitutionName,
                    Trader = e.Entry.TraderName,
                    // 决策 #2 兑现：长城交易员以姓名显示（映射不到时保留主键）
                    OwnerTrader = Lookup(userNames, e.OwnerUserId),
                    Time = e.UpdateTime.HasValue ? e.UpdateTime.Value.ToString("HH:mm:ss") : "--",
                    IsMine = userId != null && e.OwnerUserId == userId,
                    QuoteId = e.QuoteId,
                })
                .ToList();
        }

        private sealed class BestEntry
        {
            public MarketDataEntry Entry;
            public DateTime? UpdateTime;
            public decimal Price;
            public decimal? Quantity;
            public string OwnerUserId;
            public string QuoteId;
        }

        #endregion

        #region 报价提醒（notification 域）

        public async Task<IReadOnlyList<AlertRow>> LoadAlertRowsAsync(ProductFilter filter, int page, int pageSize, CancellationToken ct)
        {
            var quoteFilter = new QuoteFilter
            {
                Conditions = MapConditions(filter),
                WatchlistIds = new List<string>(),
                FilterGroupIds = new List<string>(),
            };
            var notifications = await alertView.OpenAsync(quoteFilter, "ALL", 200, ct).ConfigureAwait(false);

            // 深页补拉（同看板 EnsureRangeAsync；通知视图页大小 200）
            var need = Math.Max(1, page) * Math.Max(1, pageSize);
            var covered = (int)Math.Ceiling(notifications.Count / 200.0);
            var lastNeeded = (int)Math.Ceiling(need / 200.0);
            for (var p = covered + 1; p <= lastNeeded && notifications.Count < alertView.TotalCount; p++)
            {
                var more = await alertView.QueryPageAsync(p, ct).ConfigureAwait(false);
                notifications = notifications.Concat(more).ToList();
            }

            var slice = notifications.Skip((Math.Max(1, page) - 1) * Math.Max(1, pageSize)).Take(Math.Max(1, pageSize));
            var rows = slice.Select(MapAlert).ToList();
            // 通知行本身不带估值/期限（决策 #3 显示空）；债券简称由 securityId 即本行已有，无需补取
            return rows;
        }

        /// <summary>标记已读（ficc.notification.read，H30）：先本地置位让 UI 立即反映，再上报。</summary>
        public async Task MarkAlertReadAsync(string notificationId, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(notificationId)) return;
            alertView.MarkReadLocally(notificationId);
            var request = new NotificationReadRequest
            {
                NotificationIds = new List<string> { notificationId },
                Read = true,
            };
            var data = await requester.RequestAsync<NotificationReadRequest, NotificationReadData>(
                GatewayTopics.NotificationRead, request, ct).ConfigureAwait(false);
            var failures = (data.Results ?? new List<NotificationReadResult>())
                .Where(r => r != null && r.Code != "0000")
                .Select(r => (r.NotificationId ?? "?") + "：" + (r.Msg ?? r.Code))
                .ToList();
            if (failures.Count > 0)
            {
                throw new GatewayApiException("gateway", string.Join("；", failures));
            }
        }

        /// <summary>NotificationRecord → AlertRow。报价摘要字段后端不携带（决策 #3 显示空）。</summary>
        private static AlertRow MapAlert(NotificationRecord record)
        {
            if (record == null) return null;
            return new AlertRow
            {
                NotificationId = record.NotificationId,
                Types = MapAlertTypes(record.Reasons),
                SecurityId = record.SecurityId,
                Content = record.Content,
                QuoteIds = record.QuoteIds,
                ReadTime = record.ReadTime,
                UpdateTime = record.UpdateTime,
            };
        }

        /// <summary>reasons[].type 四类映射；同券非对价原因后端已合并，一行多类型取去重集合。</summary>
        private static IReadOnlyList<AlertType> MapAlertTypes(List<NotificationReason> reasons)
        {
            if (reasons == null) return new AlertType[0];
            return reasons
                .Where(r => r != null)
                .Select(r =>
                {
                    switch (r.Type)
                    {
                        case "FILTER_GROUP": return AlertType.FilterGroup;
                        case "COUNTER_QUOTE": return AlertType.PriceMatch;
                        case "MY_BEST": return AlertType.MyBestQuote;
                        default: return AlertType.FavoriteBond;
                    }
                })
                .Distinct()
                .ToList();
        }

        #endregion

        #region 模糊报价看板（R9 无后端）

        public Task<IReadOnlyList<BoardIntentRow>> LoadIntentRowsAsync(DateTime start, DateTime end, CancellationToken ct)
        {
            return Task.FromException<IReadOnlyList<BoardIntentRow>>(
                new NotSupportedException("模糊报价看板：后端接口未提供（规范 R9）"));
        }

        public Task SetIntentStatusAsync(IReadOnlyCollection<string> intentIds, IntentStatus status, CancellationToken ct)
        {
            return Task.FromException(new NotSupportedException("模糊报价状态更新：后端接口未提供（规范 R9）"));
        }

        #endregion

        #region 债券识别回填 / 推送事件

        /// <summary>按代码查债券库（ficc.base.query SECURITY 联想，H43）；支持全码/去后缀/尾码。</summary>
        public async Task<BondLookupEntry> LookupBondAsync(string code, CancellationToken ct)
        {
            var token = (code ?? string.Empty).Trim();
            var entry = new BondLookupEntry { Code = token };
            if (token.Length == 0) return entry;

            var request = new BaseQueryRequest { Kind = "SECURITY", Text = token, Limit = 20 };
            var data = await requester.RequestAsync<BaseQueryRequest, BaseQueryData>(
                GatewayTopics.BaseQuery, request, ct).ConfigureAwait(false);

            var record = (data.Records ?? new List<BaseRecord>())
                .FirstOrDefault(r => r != null && r.SecurityId != null && MatchesCode(r.SecurityId, token));
            if (record == null) return entry;

            entry.Found = true;
            entry.Name = record.SecurityName;
            entry.Tenor = record.RemainingYears;
            entry.BondLibrary = record.BondSubtype;
            var yieldText = record.ValuationYield;
            decimal valuation;
            entry.Valuation = decimal.TryParse(yieldText, NumberStyles.Number, CultureInfo.InvariantCulture, out valuation)
                ? valuation.ToString("0.0000", CultureInfo.InvariantCulture) + "%"
                : "—";
            return entry;
        }

        /// <summary>全码 / 去后缀 / 尾部短码三种写法匹配。</summary>
        private static bool MatchesCode(string securityId, string token)
        {
            if (string.Equals(securityId, token, StringComparison.OrdinalIgnoreCase)) return true;
            var bare = token.EndsWith(".IB", StringComparison.OrdinalIgnoreCase) || token.EndsWith(".SH", StringComparison.OrdinalIgnoreCase)
                ? token.Substring(0, token.Length - 3)
                : token;
            if (string.Equals(securityId, bare, StringComparison.OrdinalIgnoreCase)) return true;
            return bare.Length >= 4 && securityId.EndsWith(bare, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>看板推送变化 → QuoteEvent（看板 VM 收事件全量重查，UI 零改动）。</summary>
        public void SubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            if (handler == null) return;
            lock (sync) quoteEventHandlers.Add(handler);
        }

        public void UnsubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            lock (sync) quoteEventHandlers.Remove(handler);
        }

        private void PublishQuoteEvent(string quoteId)
        {
            List<Action<QuoteEvent>> handlers;
            lock (sync) handlers = new List<Action<QuoteEvent>>(quoteEventHandlers);
            if (handlers.Count == 0) return;
            // 横幅计数基准：本批触及（revision 变化/新增）的报价数，而非广播次数
            var message = new QuoteEvent
            {
                Kind = QuoteEventKind.Edit,
                QuoteId = quoteId,
                ChangedCount = SnapshotTouchedQuoteIds().Count,
            };
            foreach (var handler in handlers)
            {
                try { handler(message); }
                catch { /* 单个订阅方异常不影响其余 */ }
            }
        }

        #endregion

        #region 公共

        /// <summary>取（或懒建）指定市场的域视图并打开，返回视图实例（TotalCount/补拉用）。
        /// 侧栏面板条件、激活分组 Id 与工具栏过滤（side/搜索词/旗标）全部下推——视图行集=过滤后行集。</summary>
        private async Task<PriceLiveView> GetView(Dictionary<BondMarket, PriceLiveView> cache, MarketBoardQuery query, string mode, CancellationToken ct)
        {
            var market = query.Market;
            PriceLiveView view;
            lock (sync)
            {
                if (!cache.TryGetValue(market, out view))
                {
                    view = new PriceLiveView(requester, priceStream);
                    view.Changed += () => _quoteEventCoalescer.Signal();
                    cache[market] = view;
                }
            }
            var conditions = MapConditions(query.ProductFilter);

            // 工具栏过滤下推（side 字典值 买入=1/卖出=2；两端全开不加条件=不过滤）
            var sides = new List<string>();
            if (query.IncludeBid) sides.Add("1");
            if (query.IncludeOfr) sides.Add("2");
            if (sides.Count == 1)
            {
                conditions.Add(new FilterCondition { Field = "side", Operator = "IN", Value = sides });
            }
            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                conditions.Add(new FilterCondition { Field = "searchText", Operator = "CONTAINS", Value = query.SearchText.Trim() });
            }
            if (query.RequiredFlags.HasFlag(QuoteFlag.Urgent))
            {
                conditions.Add(new FilterCondition { Field = "urgent", Operator = "EQ", Value = true });
            }
            if (query.RequiredFlags.HasFlag(QuoteFlag.Recommend))
            {
                conditions.Add(new FilterCondition { Field = "recommended", Operator = "EQ", Value = true });
            }
            if (query.RequiredFlags.HasFlag(QuoteFlag.Whole))
            {
                conditions.Add(new FilterCondition { Field = "whole", Operator = "EQ", Value = true });
            }
            var activeGroups = Shared.Services.MarketGroupSelectionService.ActiveGroupIds(
                market == BondMarket.Rate ? Shared.Services.FilterProfileService.MarketKind.Rate : Shared.Services.FilterProfileService.MarketKind.Credit);
            var filter = new QuoteFilter
            {
                Market = market == BondMarket.Rate ? "RATE" : "CREDIT",
                Conditions = conditions,
                WatchlistIds = activeGroups.Item1,
                FilterGroupIds = activeGroups.Item2,
            };
            await view.OpenAsync(filter, mode, 200, ct).ConfigureAwait(false);
            return view;
        }

        /// <summary>按需补拉视图块：目标切片末端超出已拉行数且服务端还有更多时，经 view.query
        /// 逐块拉齐（每块 200 行，合并进视图缓存）；返回补拉后的全量已拉行。</summary>
        private static async Task<IReadOnlyList<PriceBookRow>> EnsureRangeAsync(PriceLiveView view, int page, int pageSize, CancellationToken ct)
        {
            var need = Math.Max(1, page) * Math.Max(1, pageSize);
            var rows = view.Rows;
            if (rows.Count >= need || view.TotalCount <= rows.Count) return rows;

            const int viewPageSize = 200;
            var coveredPages = (int)Math.Ceiling(rows.Count / (double)viewPageSize);
            var lastNeeded = (int)Math.Ceiling((double)need / viewPageSize);
            for (var p = coveredPages + 1; p <= lastNeeded && rows.Count < view.TotalCount; p++)
            {
                await view.QueryPageAsync(p, ct).ConfigureAwait(false);
                rows = view.Rows;
            }
            return rows;
        }

        /// <summary>面板条件载荷 → 协议条件（无效项跳过）。</summary>
        private static List<FilterCondition> MapConditions(ProductFilter filter)
        {
            var conditions = new List<FilterCondition>();
            if (filter != null && filter.Conditions != null)
            {
                foreach (var payload in filter.Conditions)
                {
                    if (payload == null || payload.Field == null || payload.Operator == null) continue;
                    conditions.Add(new FilterCondition { Field = payload.Field, Operator = payload.Operator, Value = payload.Value });
                }
            }
            return conditions;
        }

        /// <summary>客户端分页切片（第一页 books 展开行数可能超过 pageSize）。</summary>
        private static PagedResult<T> Paginate<T>(List<T> rows, MarketBoardQuery query, int totalCount)
        {
            var page = Math.Max(1, query.Page);
            var size = Math.Max(1, query.PageSize);
            return new PagedResult<T>
            {
                Items = rows.Skip((page - 1) * size).Take(size).ToList(),
                TotalCount = totalCount > rows.Count ? totalCount : rows.Count,
                QueryId = null,
            };
        }

        #endregion

        public void Dispose()
        {
            alertView.Dispose();
            lock (sync)
            {
                foreach (var view in fullViews.Values) view.Dispose();
                foreach (var view in bestViews.Values) view.Dispose();
                fullViews.Clear();
                bestViews.Clear();
            }
            priceStream.Dispose();
            notificationStream.Dispose();
        }
    }
}
