using System;
using System.Collections.Generic;
using System.Linq;
using STC.SalesTrading.Plugin.Abstractions.Models;

namespace STC.SalesTrading.Plugin.Mock
{
    /// <summary>
    /// 销交平台进程内 Mock 数据仓库。三个模块共享同一实例，模拟服务端持久状态，
    /// 避免确认、删除、转模糊报价后重新查询又恢复为初始样例数据。
    /// </summary>
    public sealed class MockSalesTradingStore
    {
        private readonly object _sync = new object();
        private bool _pendingSeeded;
        private bool _boardSeeded;
        private bool _todaySeeded;
        private bool _historySeeded;
        private bool _intentSeeded;
        private bool _alertSeeded;
        private long _stateRevision;
        private event Action<QuoteEvent> QuoteChanged;

        private readonly List<QuoteConfirmRow> _pendingQuotes = new List<QuoteConfirmRow>();
        private readonly List<BoardQuoteRow> _boardQuotes = new List<BoardQuoteRow>();
        private readonly List<MgmtQuoteRow> _todayQuotes = new List<MgmtQuoteRow>();
        private readonly List<MgmtQuoteRow> _historyQuotes = new List<MgmtQuoteRow>();
        private readonly List<BoardIntentRow> _intentRows = new List<BoardIntentRow>();
        private readonly List<BoardAlertRow> _alertRows = new List<BoardAlertRow>();

        public static MockSalesTradingStore Shared { get; } = new MockSalesTradingStore();

        internal void EnsurePending(Func<IEnumerable<QuoteConfirmRow>> seed)
        {
            lock (_sync)
            {
                if (_pendingSeeded) return;
                _pendingQuotes.AddRange(seed());
                _pendingSeeded = true;
            }
        }

        internal void EnsureBoard(Func<IEnumerable<BoardQuoteRow>> seed)
        {
            lock (_sync)
            {
                if (_boardSeeded) return;
                _boardQuotes.AddRange(seed());
                _boardSeeded = true;
            }
        }

        internal void EnsureToday(Func<IEnumerable<MgmtQuoteRow>> seed)
        {
            lock (_sync)
            {
                if (_todaySeeded) return;
                _todayQuotes.AddRange(seed());
                _todaySeeded = true;
            }
        }

        internal void EnsureHistory(Func<IEnumerable<MgmtQuoteRow>> seed)
        {
            lock (_sync)
            {
                if (_historySeeded) return;
                _historyQuotes.AddRange(seed());
                _historySeeded = true;
            }
        }

        internal void EnsureIntents(Func<IEnumerable<BoardIntentRow>> seed)
        {
            lock (_sync)
            {
                if (_intentSeeded) return;
                _intentRows.AddRange(seed());
                _intentSeeded = true;
            }
        }

        internal void EnsureAlerts(Func<IEnumerable<BoardAlertRow>> seed)
        {
            lock (_sync)
            {
                if (_alertSeeded) return;
                _alertRows.AddRange(seed());
                _alertSeeded = true;
            }
        }

        internal List<QuoteConfirmRow> PendingSnapshot()
        {
            lock (_sync) return _pendingQuotes.ToList();
        }

        internal List<BoardQuoteRow> BoardSnapshot(BondMarket market)
        {
            lock (_sync) return _boardQuotes.Where(row => row.Market == market).ToList();
        }

        internal List<MgmtQuoteRow> TodaySnapshot()
        {
            lock (_sync) return _todayQuotes.ToList();
        }

        internal List<MgmtQuoteRow> HistorySnapshot(DateTime start, DateTime end)
        {
            lock (_sync)
            {
                return _historyQuotes.Where(row => row.QuoteTime.HasValue
                    && row.QuoteTime.Value.Date >= start.Date
                    && row.QuoteTime.Value.Date <= end.Date).ToList();
            }
        }

        internal List<BoardIntentRow> IntentSnapshot(DateTime start, DateTime end)
        {
            lock (_sync)
            {
                return _intentRows.Where(row => !row.QuoteTime.HasValue
                    || row.QuoteTime.Value.Date >= start.Date && row.QuoteTime.Value.Date <= end.Date).ToList();
            }
        }

        internal List<BoardAlertRow> AlertSnapshot()
        {
            lock (_sync) return _alertRows.ToList();
        }

        internal void ConfirmPending(IReadOnlyCollection<string> recognitionIds)
        {
            if (recognitionIds == null || recognitionIds.Count == 0) return;
            List<string> quoteIds;
            lock (_sync)
            {
                var idSet = new HashSet<string>(recognitionIds.Where(id => !string.IsNullOrWhiteSpace(id)));
                var rows = _pendingQuotes.Where(row => idSet.Contains(row.RecognitionId)).ToList();
                quoteIds = AddConfirmedRowsCore(rows);
                _pendingQuotes.RemoveAll(row => idSet.Contains(row.RecognitionId));
            }
            PublishQuoteEvents(quoteIds, QuoteEventKind.New);
        }

        internal void SubmitQuotes(IReadOnlyCollection<QuoteConfirmRow> rows)
        {
            if (rows == null || rows.Count == 0) return;
            List<string> quoteIds;
            lock (_sync) quoteIds = AddConfirmedRowsCore(rows);
            PublishQuoteEvents(quoteIds, QuoteEventKind.New);
        }

        internal void DeletePending(IReadOnlyCollection<string> recognitionIds)
        {
            if (recognitionIds == null || recognitionIds.Count == 0) return;
            lock (_sync)
            {
                var idSet = new HashSet<string>(recognitionIds.Where(id => !string.IsNullOrWhiteSpace(id)));
                _pendingQuotes.RemoveAll(row => idSet.Contains(row.RecognitionId));
            }
        }

        internal void DeleteManagedQuotes(IReadOnlyCollection<string> quoteIds, bool history)
        {
            if (quoteIds == null || quoteIds.Count == 0) return;
            lock (_sync)
            {
                var idSet = new HashSet<string>(quoteIds.Where(id => !string.IsNullOrWhiteSpace(id)));
                var source = history ? _historyQuotes : _todayQuotes;
                var deletedRows = source.Where(row => idSet.Contains(row.QuoteId)).ToList();
                source.RemoveAll(row => idSet.Contains(row.QuoteId));
                if (!history)
                {
                    _boardQuotes.RemoveAll(row => idSet.Contains(row.QuoteId)
                        || deletedRows.Any(deleted => deleted.Code == row.Code && deleted.Direction == row.Direction));
                }
            }
            if (!history) PublishQuoteEvents(quoteIds, QuoteEventKind.Revoke);
        }

        internal void SubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            if (handler == null) return;
            lock (_sync) QuoteChanged += handler;
        }

        internal void UnsubscribeQuoteEvents(Action<QuoteEvent> handler)
        {
            if (handler == null) return;
            lock (_sync) QuoteChanged -= handler;
        }

        internal void AddIntents(IReadOnlyCollection<BoardIntentRow> rows)
        {
            if (rows == null || rows.Count == 0) return;
            lock (_sync)
            {
                foreach (var row in rows)
                {
                    if (string.IsNullOrWhiteSpace(row.IntentId)) row.IntentId = "mock-intent-" + Guid.NewGuid().ToString("N");
                    if (!row.QuoteTime.HasValue) row.QuoteTime = DateTime.Now;
                    _intentRows.RemoveAll(existing => existing.IntentId == row.IntentId);
                    _intentRows.Insert(0, row);
                }
            }
        }

        internal void SetIntentStatus(IReadOnlyCollection<string> intentIds, IntentStatus status)
        {
            if (intentIds == null || intentIds.Count == 0) return;
            lock (_sync)
            {
                var idSet = new HashSet<string>(intentIds.Where(id => !string.IsNullOrWhiteSpace(id)));
                foreach (var row in _intentRows.Where(row => idSet.Contains(row.IntentId)))
                {
                    row.Status = status;
                    row.IsPendingSync = false;
                    row.DataVersion = (row.DataVersion ?? 0) + 1;
                }
            }
        }

        private List<string> AddConfirmedRowsCore(IEnumerable<QuoteConfirmRow> rows)
        {
            var quoteIds = new List<string>();
            foreach (var row in rows)
            {
                var quoteId = string.IsNullOrWhiteSpace(row.RecognitionId)
                    ? "mock-quote-" + Guid.NewGuid().ToString("N")
                    : "mock-quote-" + row.RecognitionId;
                quoteIds.Add(quoteId);
                var market = IsRateBond(row) ? BondMarket.Rate : BondMarket.Credit;
                _boardQuotes.RemoveAll(existing => existing.QuoteId == quoteId);
                _boardQuotes.Insert(0, ToBoardRow(row, quoteId, market));
                _todayQuotes.RemoveAll(existing => existing.QuoteId == quoteId);
                _todayQuotes.Insert(0, ToMgmtRow(row, quoteId));
            }
            return quoteIds;
        }

        private void PublishQuoteEvents(IEnumerable<string> quoteIds, QuoteEventKind kind)
        {
            foreach (var quoteId in quoteIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
            {
                Action<QuoteEvent> handlers;
                long revision;
                lock (_sync)
                {
                    handlers = QuoteChanged;
                    revision = ++_stateRevision;
                }
                handlers?.Invoke(new QuoteEvent { Kind = kind, QuoteId = quoteId, StateRevision = revision });
            }
        }

        private static bool IsRateBond(QuoteConfirmRow row)
        {
            var name = row.Name ?? string.Empty;
            return name.Contains("国债") || name.Contains("国开") || name.Contains("农发") || name.Contains("进出");
        }

        private static BoardQuoteRow ToBoardRow(QuoteConfirmRow row, string quoteId, BondMarket market)
        {
            return new BoardQuoteRow
            {
                Market = market,
                QuoteId = quoteId,
                Maturity = row.Maturity,
                Code = row.Code,
                Name = row.Name,
                Direction = row.Direction,
                PriceType = row.PriceType,
                Settlement = row.Settlement,
                Price = row.Price,
                Volume = row.Volume,
                Institution = row.Institution,
                Trader = row.Trader,
                WallTrader = "当前用户",
                BondLibrary = row.Code != null && row.Code.EndsWith(".SH", StringComparison.OrdinalIgnoreCase) ? "交易所" : "银行间",
                NzNetPrice = row.NzNetPrice,
                NzDeviation = row.NzDeviation,
                QuoteTime = DateTime.Now,
                Flags = row.Flags,
                IsMine = true,
                OwnerAccountId = "mock-current-user",
            };
        }

        private static MgmtQuoteRow ToMgmtRow(QuoteConfirmRow row, string quoteId)
        {
            return new MgmtQuoteRow
            {
                QuoteId = quoteId,
                Mode = string.Equals(row.SourceType, "qt", StringComparison.OrdinalIgnoreCase) ? QuoteMode.Qt : QuoteMode.Ideal,
                Status = QuoteStatus.Active,
                Maturity = row.Maturity,
                Code = row.Code,
                Name = row.Name,
                Direction = row.Direction,
                Price = row.Price,
                Volume = row.Volume,
                Flags = row.Flags,
                Institution = row.Institution,
                Trader = row.Trader,
                WallTrader = "当前用户",
                BondLibrary = row.Code != null && row.Code.EndsWith(".SH", StringComparison.OrdinalIgnoreCase) ? "交易所" : "银行间",
                NzNetPrice = row.NzNetPrice,
                NzDeviation = row.NzDeviation,
                QuoteTime = DateTime.Now,
            };
        }
    }
}
