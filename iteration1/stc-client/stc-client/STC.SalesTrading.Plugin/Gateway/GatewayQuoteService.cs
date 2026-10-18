using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Gateway.Contracts.Base;
using STC.SalesTrading.Plugin.Gateway.Contracts.Parser;
using STC.SalesTrading.Plugin.Gateway.Contracts.Quote;
using STC.SalesTrading.Plugin.Gateway.Contracts.View;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Gateway
{
    /// <summary>
    /// 报价录入域服务（IQuoteService 网关实现，批次 4）。
    /// 待确认列表走 quote 域 LiveView（status=PENDING_CONFIRM、ownerScope=MINE，推送自动维护）；
    /// 确认/删除走"确认并修正"与逻辑删除（expectedRevision 乐观锁，批量逐条校验）；
    /// 精确识别调用 ParserSvr / stc.parser.bond，失败直接返回 UI；
    /// 模糊报价后端无此域（R9），明确抛出不支持。
    /// </summary>
    public sealed class GatewayQuoteService : IQuoteService, IDisposable
    {
        private readonly GatewayRequester requester;
        private readonly GatewayReportStream<Contracts.View.QuoteStreamBatch> stream;
        private readonly QuoteLiveView pendingView;
        private readonly SemaphoreSlim submissionLock = new SemaphoreSlim(1, 1);
        private SubmissionWrite<QuoteCreateRequest> pendingCreate;
        private SubmissionWrite<QuoteChangeRequest> pendingConfirm;

        // 通信结果未知时保留原始请求及 requestId；用户重试先取得同一操作的回执。
        private sealed class SubmissionWrite<TRequest>
        {
            public TRequest Request;
            public List<QuoteRow> Rows;
            public readonly string RequestId = "STC-" + Guid.NewGuid().ToString("N");
        }

        /// <summary>待确认集合推送变化（LiveView 行集合变化直接转发）。</summary>
        public event Action QuotesChanged
        {
            add { pendingView.Changed += value; }
            remove { pendingView.Changed -= value; }
        }

        internal GatewayQuoteService(GatewayRequester requester, GatewayReportStream<Contracts.View.QuoteStreamBatch> stream)
        {
            this.requester = requester ?? throw new ArgumentNullException(nameof(requester));
            this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
            pendingView = new QuoteLiveView(requester, stream);
            // 断线/缺号 → 数据源复位广播（各 VM 清缓存重查；重登后流可重建）
            stream.Failed += reason => STC.Common.Utils.Events.EventAggregator.Publish(
                new STC.SalesTrading.Plugin.Shared.Messages.DataSourceResetMessage());
        }

        /// <summary>待确认行（ficc.quote.view.open，status=["PENDING_CONFIRM"]）。
        /// 债券资料优先取视图/推送附带 reference（决策 #9 已落地），缺失部分 base.get 兜底。</summary>
        public async Task<IReadOnlyList<QuoteRow>> LoadPendingRowsAsync(CancellationToken ct)
        {
            var records = await pendingView.OpenAsync(new QuoteFilter(), new List<string> { "PENDING_CONFIRM" }, "MINE", 200, ct).ConfigureAwait(false);
            var reference = pendingView.LastReference;
            var rows = records.Select(r => GatewayModelMapper.ToQuoteRow(r, RefAt(reference, r.SecurityId))).ToList();
            await GatewayReferenceFill.EnsureAsync(requester, rows, ct).ConfigureAwait(false);
            return rows;
        }

        private static QuoteReference RefAt(IReadOnlyDictionary<string, QuoteReference> reference, string securityId)
        {
            QuoteReference value;
            return securityId != null && reference != null && reference.TryGetValue(securityId, out value)
                ? value : null;
        }

        /// <summary>按 Id 拉完整报价（ficc.quote.get，编辑弹窗先拉全量再回填）。</summary>
        public async Task<QuoteRow> GetQuoteAsync(string quoteId, CancellationToken ct)
        {
            var data = await requester.RequestAsync<QuoteGetRequest, QuoteGetData>(
                GatewayTopics.QuoteGet, new QuoteGetRequest { QuoteId = quoteId }, ct).ConfigureAwait(false);
            var row = GatewayModelMapper.ToQuoteRow(data != null ? data.Quote : null, null);
            if (row != null)
            {
                await GatewayReferenceFill.EnsureAsync(requester, new[] { row }, ct).ConfigureAwait(false);
            }
            return row;
        }

        /// <summary>报价变更历史（ficc.quote.history.query，H10；按 revision 升序）。</summary>
        public async Task<IReadOnlyList<QuoteLogEntry>> LoadQuoteLogAsync(string quoteId, CancellationToken ct)
        {
            var data = await requester.RequestAsync<QuoteLogRequest, QuoteLogData>(
                GatewayTopics.QuoteHistoryQuery,
                new QuoteLogRequest { QuoteId = quoteId, Limit = 100 }, ct).ConfigureAwait(false);
            return (data.Records ?? new List<QuoteLogRecord>())
                .Where(r => r != null)
                .Select(r => new QuoteLogEntry
                {
                    OperationTime = r.OperationTime,
                    Action = r.Action,
                    Revision = r.Revision,
                    OperatorUserId = r.OperatorUserId,
                    Reason = r.Reason,
                })
                .ToList();
        }

        /// <summary>确认并修正（ficc.quote.confirm，H02）：行上的编辑值 + CRM Id 随确认提交。</summary>
        public Task ConfirmQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            if (rows == null || rows.Count == 0) return Task.CompletedTask;
            var request = new QuoteChangeRequest
            {
                Quotes = rows.Select(ToConfirmItem).ToList(),
            };
            return ExecuteWriteAsync(GatewayTopics.QuoteConfirm, request, ct);
        }

        /// <summary>逐项处理批量回执；只移除已确认行，失败行保留身份及用户编辑，允许继续确认。</summary>
        public async Task SubmitQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            if (rows == null || rows.Count == 0) return;
            await submissionLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var failures = new Dictionary<QuoteRow, string>();
                // 先消除上一次通信失败的不确定性，再接受本次新操作，禁止换 requestId 盲目重建。
                if (pendingCreate != null) await CompleteCreateAsync(failures, ct).ConfigureAwait(false);
                if (pendingConfirm != null) await CompleteConfirmAsync(failures, ct).ConfigureAwait(false);
                foreach (var row in rows)
                {
                    if ((!string.IsNullOrWhiteSpace(row.InstitutionName) && string.IsNullOrWhiteSpace(row.InstitutionId))
                        || (!string.IsNullOrWhiteSpace(row.ContactName) && string.IsNullOrWhiteSpace(row.ContactId)))
                        failures[row] = "请选择下拉列表中的报价机构/交易员";
                }
                var creating = rows.Where(row => string.IsNullOrEmpty(row.QuoteId) && !failures.ContainsKey(row)).ToList();
                if (creating.Count > 0)
                {
                    pendingCreate = new SubmissionWrite<QuoteCreateRequest>
                    {
                        Rows = creating,
                        Request = new QuoteCreateRequest { Quotes = creating.Select(ToCreateInput).ToList() }
                    };
                    await CompleteCreateAsync(failures, ct).ConfigureAwait(false);
                }
                var confirming = rows.Where(row => !string.IsNullOrEmpty(row.QuoteId)
                    && row.Status == QuoteStatus.PendingConfirm && !failures.ContainsKey(row)).ToList();
                if (confirming.Count > 0)
                {
                    pendingConfirm = new SubmissionWrite<QuoteChangeRequest>
                    {
                        Rows = confirming,
                        Request = new QuoteChangeRequest { Quotes = confirming.Select(ToConfirmItem).ToList() }
                    };
                    await CompleteConfirmAsync(failures, ct).ConfigureAwait(false);
                }
                if (failures.Count > 0)
                    throw new GatewayApiException("PARTIAL_FAILURE", string.Join("；", failures.Select(
                        failure => failure.Key.SecurityId + "：" + failure.Value)));
            }
            finally
            {
                submissionLock.Release();
            }
        }

        private async Task CompleteCreateAsync(Dictionary<QuoteRow, string> failures, CancellationToken ct)
        {
            var attempt = pendingCreate;
            QuoteWriteData data;
            try
            {
                data = await requester.RequestAsync<QuoteCreateRequest, QuoteWriteData>(
                    GatewayTopics.QuoteCreate, attempt.Request, ct, attempt.RequestId).ConfigureAwait(false);
            }
            catch (GatewayApiException error) when (error.Code != "gateway")
            {
                pendingCreate = null;
                throw;
            }
            ApplySubmissionResults(attempt.Rows, data, false, failures);
            pendingCreate = null;
        }

        private async Task CompleteConfirmAsync(Dictionary<QuoteRow, string> failures, CancellationToken ct)
        {
            var attempt = pendingConfirm;
            QuoteWriteData data;
            try
            {
                data = await requester.RequestAsync<QuoteChangeRequest, QuoteWriteData>(
                    GatewayTopics.QuoteConfirm, attempt.Request, ct, attempt.RequestId).ConfigureAwait(false);
            }
            catch (GatewayApiException error) when (error.Code != "gateway")
            {
                pendingConfirm = null;
                throw;
            }
            ApplySubmissionResults(attempt.Rows, data, true, failures);
            pendingConfirm = null;
        }

        private static void ApplySubmissionResults(List<QuoteRow> rows, QuoteWriteData data,
            bool confirming, Dictionary<QuoteRow, string> failures)
        {
            if (data?.Results == null || data.Results.Count != rows.Count)
                throw new GatewayApiException("gateway", "报价回执条数不匹配，请重试核对原请求");
            foreach (var row in rows)
            {
                var matches = data.Results.Where(result => result != null && (confirming
                    ? result.QuoteId == row.QuoteId : result.ClientQuoteId == row.ClientQuoteId)).ToList();
                if (matches.Count != 1)
                    throw new GatewayApiException("gateway", "报价回执身份不匹配，请重试核对原请求");
                var result = matches[0];
                if (result.Code != "0000")
                {
                    failures[row] = result.Msg ?? result.Code;
                    continue;
                }
                if (string.IsNullOrEmpty(result.QuoteId) || string.IsNullOrEmpty(result.Revision))
                    throw new GatewayApiException("gateway", "报价回执缺少 quoteId/revision");
                row.QuoteId = result.QuoteId;
                row.Revision = result.Revision;
                row.Status = confirming ? QuoteStatus.Active : QuoteStatus.PendingConfirm;
            }
        }

        /// <summary>修改报价（ficc.quote.update，H03）。1004 冲突以 GatewayApiException 呈现，UI 提示重开不自动覆盖。</summary>
        public Task UpdateQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            if (rows == null || rows.Count == 0) return Task.CompletedTask;
            var request = new QuoteChangeRequest
            {
                Quotes = rows.Select(r => new QuoteChangeItem
                {
                    QuoteId = r.QuoteId,
                    ExpectedRevision = r.Revision,
                    Side = GatewayModelMapper.SideToCode(r.Side),
                    QuoteType = GatewayModelMapper.QuoteTypeToCode(r.QuoteType),
                    YieldType = r.YieldType.HasValue ? GatewayModelMapper.YieldTypeToCode(r.YieldType) : null,
                    Price = FormatDecimal(r.Price),
                    Quantity = FormatDecimal(r.Quantity),
                    QuantityUnit = "WAN",
                    Currency = "CNY",
                    TradeDate = FormatDate(r.TradeDate),
                    SettlType = GatewayModelMapper.SettlTypeToCode(r.SettlType),
                    InstitutionId = r.InstitutionId,
                    InstitutionName = r.InstitutionName,
                    ContactId = r.ContactId,
                    ContactName = r.ContactName,
                    Urgent = r.Urgent,
                    Recommended = r.Recommended,
                    Whole = r.Whole,
                    Oco = r.Oco,
                    Packing = r.Packing,
                    Remark = r.Remark,
                }).ToList(),
            };
            return ExecuteWriteAsync(GatewayTopics.QuoteUpdate, request, ct);
        }

        /// <summary>删除待确认报价（ficc.quote.delete，H06；expectedRevision 必填）。</summary>
        public Task DeleteQuotesAsync(IReadOnlyCollection<QuoteRow> rows, CancellationToken ct)
        {
            if (rows == null || rows.Count == 0) return Task.CompletedTask;
            var request = new QuoteLifecycleRequest
            {
                Quotes = rows.Select(r => new QuoteLifecycleItem
                {
                    QuoteId = r.QuoteId,
                    ExpectedRevision = r.Revision,
                    Reason = "用户操作",
                }).ToList(),
            };
            return ExecuteWriteAsync(GatewayTopics.QuoteDelete, request, ct);
        }

        /// <summary>
        /// 精确文本识别：主路走 STC 老识别服务 ParserSvr / stc.parser.bond（现存已上线，
        /// 协议为 Code/Msg/Datas 老壳、priceType/settlType 为 ParserSvr 私有编码）；
        /// ParserSvr 不可用时呈现错误，保留输入供用户重试。
        /// </summary>
        public async Task<QuoteParseResult> ParsePreciseTextAsync(string text, CancellationToken ct)
        {
            var userName = requester.CurrentUserName;
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new GatewayApiException("gateway", "STC 登录用户尚未就绪，无法调用识别服务");
            }

            var request = new ParserRequest { UserName = userName, Content = text };
            // 识别依赖失败必须呈现原始失败，不用本地简析替代真实 ParserSvr 结果。
            var reply = await requester.RequestLegacyAsync<ParserRequest, ParserReply>(
                "stc.parser.bond", request, ct).ConfigureAwait(false);

            if (reply == null || reply.Code != "0000")
            {
                throw new GatewayApiException("gateway",
                    "ParserSvr " + (reply != null ? reply.Code : "?") + "：" + (reply != null ? reply.Msg : "未返回应答"));
            }

            var rows = new List<QuoteRow>();
            var warnings = new List<string>(reply.Warnings ?? new List<string>());
            foreach (var item in reply.Datas ?? new List<ParserQuote>())
            {
                if (item == null) continue;
                var row = MapParserQuote(item);
                rows.Add(row);

                if (string.IsNullOrWhiteSpace(row.SecurityId) || !row.Price.HasValue || !row.Quantity.HasValue)
                {
                    warnings.Add("第 " + rows.Count + " 条报价字段不完整，请核对后提交。");
                }
                if ((!string.IsNullOrWhiteSpace(row.InstitutionName) && string.IsNullOrWhiteSpace(row.InstitutionId))
                    || (!string.IsNullOrWhiteSpace(row.ContactName) && string.IsNullOrWhiteSpace(row.ContactId)))
                {
                    warnings.Add("第 " + rows.Count + " 条机构或联系人尚未匹配 CRM，确认前须选择对应记录。");
                }
            }
            if (rows.Count == 0)
            {
                warnings.Add("未识别到报价，请检查原文后重试。");
            }

            return new QuoteParseResult
            {
                Rows = rows,
                Warnings = warnings,
                SucceededCount = rows.Count,
                FailedCount = 0,
                Elapsed = TimeSpan.Zero,
            };
        }

        /// <summary>
        /// ParserSvr 行 → QuoteRow。编码为 ParserSvr 私有口径（勿与 FICC 侧转换混用）：
        /// priceType "1"=净价 "2"=全价 "3"=到期 "7"=行权；
        /// settlType "3"/"4" = 明日交易 T+0/T+1（TradeDate 取明天），"1"/"2" = 今日 T+0/T+1。
        /// </summary>
        private static QuoteRow MapParserQuote(ParserQuote item)
        {
            QuoteDirection side;
            if (item.Side == "1") side = QuoteDirection.Bid;
            else if (item.Side == "2") side = QuoteDirection.Ofr;
            else throw new GatewayApiException("gateway", "识别结果方向不支持：" + item.Side);

            PriceType quoteType;
            switch (item.PriceType)
            {
                case "1": quoteType = PriceType.CleanPrice; break;
                case "2": quoteType = PriceType.DirtyPrice; break;
                case "7": quoteType = PriceType.YieldToExercise; break;
                case "3": quoteType = PriceType.YieldToMaturity; break;
                default: throw new GatewayApiException("gateway", "识别结果计价类型不支持：" + item.PriceType);
            }

            // 清算：明+0/明+1 老口径 → 交易日期取明天 + T+0/T+1
            var tomorrow = item.SettlType == "3" || item.SettlType == "4";
            SettlementType settlement;
            switch (item.SettlType)
            {
                case "2": settlement = SettlementType.T1; break;
                case "3": settlement = SettlementType.T0; break;
                case "4": settlement = SettlementType.T1; break;
                default: settlement = SettlementType.T0; break;
            }

            YieldType? yieldType = null;
            if (quoteType == PriceType.YieldToMaturity) yieldType = YieldType.ToMaturity;
            else if (quoteType == PriceType.YieldToExercise) yieldType = YieldType.ToExercise;

            return new QuoteRow
            {
                SecurityId = item.SecurityId,
                SecurityName = item.Symbol,
                Side = side,
                QuoteType = quoteType,
                YieldType = yieldType,
                Price = GatewayModelMapper.ParseDecimal(item.Price),
                Quantity = GatewayModelMapper.ParseDecimal(item.Qty),
                TradeDate = tomorrow ? DateTime.Now.Date.AddDays(1) : DateTime.Now.Date,
                SettlType = settlement,
                InstitutionId = item.InstitutionId,
                InstitutionName = item.InstitutionName,
                ContactId = item.TraderId,
                ContactName = item.TraderName,
                Remark = item.Text,
                Maturity = item.TimeToMaturity,
                NzYtm = GatewayModelMapper.ParseDecimal(item.ChinaBondValuation),
                Status = QuoteStatus.PendingConfirm,
                SourceType = SourceType.Manual,
                CreateTime = DateTime.Now,
            };
        }

        /// <summary>模糊报价候选：后端无此域（规范 R9），明确不支持。</summary>
        public Task<IReadOnlyList<FuzzyCandidate>> QueryFuzzyCandidatesAsync(FuzzyQuery query, CancellationToken ct)
        {
            return Task.FromException<IReadOnlyList<FuzzyCandidate>>(
                new NotSupportedException("模糊报价候选查询：后端接口未提供（规范 R9，模型/接口未完成）"));
        }

        /// <summary>模糊报价提交：后端无此域（规范 R9），明确不支持。</summary>
        public Task SubmitIntentQuotesAsync(IReadOnlyCollection<BoardIntentRow> rows, CancellationToken ct)
        {
            return Task.FromException(new NotSupportedException("模糊报价提交：后端接口未提供（规范 R9）"));
        }

        #region 请求装配

        private static QuoteInput ToCreateInput(QuoteRow r)
        {
            // 生成即回写行对象：create 应答按 clientQuoteId 回绑生成的 quoteId/revision，
            // 不回写则回绑循环条件短路（row.ClientQuoteId==null）→ confirm 发 quoteId=null 被后端拒（实测踩坑）
            if (string.IsNullOrEmpty(r.ClientQuoteId))
            {
                r.ClientQuoteId = "stc-" + Guid.NewGuid().ToString("N").Substring(0, 16);
            }
            return new QuoteInput
            {
                ClientQuoteId = r.ClientQuoteId,
                SecurityId = r.SecurityId,
                InstitutionId = r.InstitutionId,
                InstitutionName = r.InstitutionName,
                ContactId = r.ContactId,
                ContactName = r.ContactName,
                Side = GatewayModelMapper.SideToCode(r.Side),
                QuoteType = GatewayModelMapper.QuoteTypeToCode(r.QuoteType),
                YieldType = r.YieldType.HasValue ? GatewayModelMapper.YieldTypeToCode(r.YieldType) : null,
                Price = FormatDecimal(r.Price),
                Quantity = FormatDecimal(r.Quantity),
                QuantityUnit = "WAN",
                Currency = "CNY",
                TradeDate = FormatDate(r.TradeDate),
                SettlType = GatewayModelMapper.SettlTypeToCode(r.SettlType),
                Urgent = r.Urgent,
                Recommended = r.Recommended,
                Whole = r.Whole,
                Oco = r.Oco,
                Packing = r.Packing,
                Remark = r.Remark,
            };
        }

        private static QuoteChangeItem ToConfirmItem(QuoteRow r)
        {
            // 确认并修正：随 confirm 提交行上的全部可编辑字段与 CRM Id（空 CRM Id 由 UI 前置拦截）
            return new QuoteChangeItem
            {
                QuoteId = r.QuoteId,
                ExpectedRevision = r.Revision,
                InstitutionId = r.InstitutionId,
                InstitutionName = r.InstitutionName,
                ContactId = r.ContactId,
                ContactName = r.ContactName,
                Side = GatewayModelMapper.SideToCode(r.Side),
                QuoteType = GatewayModelMapper.QuoteTypeToCode(r.QuoteType),
                YieldType = r.YieldType.HasValue ? GatewayModelMapper.YieldTypeToCode(r.YieldType) : null,
                Price = FormatDecimal(r.Price),
                Quantity = FormatDecimal(r.Quantity),
                QuantityUnit = "WAN",
                Currency = "CNY",
                TradeDate = FormatDate(r.TradeDate),
                SettlType = GatewayModelMapper.SettlTypeToCode(r.SettlType),
                Urgent = r.Urgent,
                Recommended = r.Recommended,
                Whole = r.Whole,
                Oco = r.Oco,
                Packing = r.Packing,
                Remark = r.Remark,
            };
        }

        private static string FormatDecimal(decimal? value)
        {
            return value.HasValue ? value.Value.ToString(CultureInfo.InvariantCulture) : null;
        }

        private static string FormatDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : null;
        }

        private async Task ExecuteWriteAsync<TRequest>(string topic, TRequest request, CancellationToken ct)
            where TRequest : class
        {
            var data = await requester.RequestAsync<TRequest, QuoteWriteData>(topic, request, ct).ConfigureAwait(false);
            RequireAllSuccess(data);
        }

        private static void RequireAllSuccess(QuoteWriteData data)
        {
            var failures = (data.Results ?? new List<QuoteOperationResult>())
                .Where(r => r != null && r.Code != "0000")
                .Select(r =>
                {
                    var message = (r.QuoteId ?? r.ClientQuoteId ?? "?") + "：" + (r.Msg ?? r.Code);
                    if (r.Error != null && r.Error.Type != null) message += "（" + r.Error.Type + (r.Error.Field != null ? "/" + r.Error.Field : "") + "）";
                    return message;
                })
                .ToList();
            if (failures.Count > 0)
            {
                throw new GatewayApiException("gateway", string.Join("；", failures));
            }
        }

        #endregion

        public void Dispose()
        {
            pendingView.Dispose();
            stream.Dispose();   // 与 Mgmt 共享，Dispose 幂等
        }
    }
}
