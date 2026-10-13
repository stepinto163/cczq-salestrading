using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.SalesTrading.Plugin.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 右侧债券详情面板的呈现模型（选中行 → 模块 VM 映射 + 服务回填多档/成交/智能推荐）。
    /// 对应原型 7 区块：detail-header / bond-header（最新成交+昨高低开+久期+中债）/ inst-filter-bar /
    /// best-quote-bar / market-tape-stack（多档+成交）/ 估值字段 / rec-panel。
    /// </summary>
    public partial class BondDetailViewModel : ObservableObject
    {
        /// <summary>无选中时的空实例。</summary>
        public static readonly BondDetailViewModel Empty = new BondDetailViewModel { HasDetail = false };

        public BondDetailViewModel()
        {
            InstQuotes = new ObservableCollection<InstQuoteRow>();
            MarketTrades = new ObservableCollection<MarketTradeRow>();

            Recommendations = new ObservableCollection<RecommendationRow>();
            RecView = CollectionViewSource.GetDefaultView(Recommendations);
            RecView.Filter = FilterRec;

            _selectedRecSide = QuoteDirection.Bid;
            _selectedRecScope = RecommendationScope.Combined;
            _selectedRecWindow = "30d";
            RefreshRec();
        }

        /// <summary>渠道筛选（原型 inst-filter-bar：全部/国利/国际/平安/中诚/上田/信唐/X-Bond/双边），值为 Venues 渠道码。</summary>
        [ObservableProperty]
        private string _selectedInst = "全部";

        /// <summary>Bid 侧报价流（价降序，最优在前；与 Ofr 侧独立筛选/配对）。</summary>
        private List<QuoteFeedLevel> _bidLevels = new List<QuoteFeedLevel>();

        /// <summary>Ofr 侧报价流（价升序，最优在前）。</summary>
        private List<QuoteFeedLevel> _ofrLevels = new List<QuoteFeedLevel>();

        /// <summary>切换渠道筛选（pill 点击）。</summary>
        public void ToggleInst(string inst)
        {
            SelectedInst = string.IsNullOrEmpty(inst) ? "全部" : inst;
            RefreshTapeRows();
        }

        /// <summary>
        /// 按渠道重建多档表：两侧**各自过滤后按序拉链**配对（Bid 价降序 / Ofr 价升序）——
        /// 被滤掉的报价不占位，后面数据上移补位；一侧更短时末尾行留空。
        /// 最优报价条跟随筛选结果（该渠道最优；无该渠道报价时显示 —）。
        /// </summary>
        private void RefreshTapeRows()
        {
            var filter = SelectedInst;
            var bids = filter == "全部" ? _bidLevels : _bidLevels.Where(l => l.VenueCode == filter).ToList();
            var ofrs = filter == "全部" ? _ofrLevels : _ofrLevels.Where(l => l.VenueCode == filter).ToList();

            InstQuotes.Clear();
            var count = Math.Max(bids.Count, ofrs.Count);
            for (var i = 0; i < count; i++)
            {
                var bid = i < bids.Count ? bids[i] : null;
                var ofr = i < ofrs.Count ? ofrs[i] : null;
                InstQuotes.Add(new InstQuoteRow
                {
                    InstBid = SourceCell(bid),
                    VolBid = bid?.Volume,
                    Bid = bid?.Price,
                    Ofr = ofr?.Price,
                    VolOfr = ofr?.Volume,
                    InstOfr = SourceCell(ofr),
                    VenueBid = bid?.VenueCode,
                    VenueOfr = ofr?.VenueCode,
                    IsBestBid = i == 0 && bid != null,
                    IsBestOfr = i == 0 && ofr != null,
                });
            }

            BidPrice = bids.Count > 0 ? bids[0].Price : null;
            BidVolume = bids.Count > 0 ? bids[0].Volume : null;
            OfrPrice = ofrs.Count > 0 ? ofrs[0].Price : null;
            OfrVolume = ofrs.Count > 0 ? ofrs[0].Volume : null;
            SpreadDisplay = bids.Count > 0 && ofrs.Count > 0
                ? ((ofrs[0].Price - bids[0].Price) * 100m).ToString("0.0")
                : "—";
        }

        [RelayCommand]
        private void SelectInstitution(string inst)
        {
            ToggleInst(inst);
        }

        [ObservableProperty]
        private bool _hasDetail;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private decimal? _lastPrice;

        /// <summary>最新成交展示（0918：收益率%优先，无收益率回退净价）。</summary>
        [ObservableProperty]
        private string _lastExecDisplay = "—";

        /// <summary>最新成交收益率；不能直接与昨收净价比较。</summary>
        [ObservableProperty]
        private decimal? _lastYield;

        [ObservableProperty]
        private decimal? _prevClose;

        [ObservableProperty]
        private decimal? _high;

        [ObservableProperty]
        private decimal? _open;

        [ObservableProperty]
        private decimal? _low;

        [ObservableProperty]
        private string _quoteTimeDisplay = string.Empty;

        [ObservableProperty]
        private decimal? _duration;

        /// <summary>中债：收益率 / 净价 组合展示（原型 bond-header 中债双值）。</summary>
        [ObservableProperty]
        private string _nzDisplay = "— / —";

        [ObservableProperty]
        private string _issuerRating = string.Empty;

        [ObservableProperty]
        private string _maturity = string.Empty;

        [ObservableProperty]
        private string _bondLibrary = string.Empty;

        [ObservableProperty]
        private decimal? _bidPrice;

        [ObservableProperty]
        private decimal? _bidVolume;

        [ObservableProperty]
        private decimal? _ofrPrice;

        [ObservableProperty]
        private decimal? _ofrVolume;

        /// <summary>价差（BP，收益率口径 = (Ofr-Bid)*100）。</summary>
        [ObservableProperty]
        private string _spreadDisplay = "—";

        /// <summary>最新成交较昨收：涨（红，中国习惯）或跌（绿）。</summary>
        public bool IsUp
        {
            get
            {
                return LastPrice.HasValue && PrevClose.HasValue && LastPrice.Value >= PrevClose.Value;
            }
        }

        /// <summary>多档行情。</summary>
        public ObservableCollection<InstQuoteRow> InstQuotes { get; }

        /// <summary>成交行情。</summary>
        public ObservableCollection<MarketTradeRow> MarketTrades { get; }

        #region 智能推荐（rec-panel）

        /// <summary>智能推荐来源行（Bid/Ofr 双侧 + 三种路径）。</summary>
        public ObservableCollection<RecommendationRow> Recommendations { get; }

        /// <summary>推荐过滤视图（方向 + 范围）。</summary>
        public ICollectionView RecView { get; }

        /// <summary>当前页推荐行。详情推荐属于局部展示分页，不替代主表服务端分页。</summary>
        public ObservableCollection<RecommendationRow> RecPageItems { get; } = new ObservableCollection<RecommendationRow>();

        /// <summary>推荐方向（Bid/Ofr 互斥开关，默认 Bid 对齐原型 active-bid）。</summary>
        [ObservableProperty]
        private QuoteDirection _selectedRecSide = QuoteDirection.Bid;

        /// <summary>推荐范围（全部/本券/相似券/相似机构，默认全部）。</summary>
        [ObservableProperty]
        private RecommendationScope _selectedRecScope = RecommendationScope.Combined;

        /// <summary>时间窗口（7d/30d/90d）；变化时重新请求真实证据。</summary>
        [ObservableProperty]
        private string _selectedRecWindow = "30d";

        /// <summary>自定义推荐查询开始日期。</summary>
        [ObservableProperty]
        private DateTime? _recStartDate;

        /// <summary>自定义推荐查询结束日期。</summary>
        [ObservableProperty]
        private DateTime? _recEndDate;

        private bool _resettingRecDates;

        /// <summary>债券搜索框文本（rec-bond-search，Mock 阶段仅展示占位）。</summary>
        [ObservableProperty]
        private string _recBondSearchText = string.Empty;

        /// <summary>Bid 侧推荐计数。</summary>
        [ObservableProperty]
        private int _recBidCount;

        /// <summary>Ofr 侧推荐计数。</summary>
        [ObservableProperty]
        private int _recOfrCount;

        /// <summary>是否有推荐行（驱动空态提示）。</summary>
        [ObservableProperty]
        private bool _hasRecommendations;

        /// <summary>推荐列表页大小。</summary>
        public int RecPageSize { get; } = 5;

        [ObservableProperty]
        private int _recCurrentPage = 1;

        [ObservableProperty]
        private int _recTotalPages = 1;

        /// <summary>分页摘要。</summary>
        [ObservableProperty]
        private string _recPageSummary = "共0条 · 第1/1页";

        public bool CanPreviousRecPage { get { return RecCurrentPage > 1; } }

        public bool CanNextRecPage { get { return RecCurrentPage < RecTotalPages; } }

        partial void OnSelectedRecSideChanged(QuoteDirection value)
        {
            RecCurrentPage = 1;
            RefreshRec();
        }

        partial void OnSelectedRecScopeChanged(RecommendationScope value)
        {
            RecCurrentPage = 1;
            RefreshRec();
        }

        partial void OnSelectedRecWindowChanged(string value)
        {
            if (!string.Equals(value, "custom", StringComparison.OrdinalIgnoreCase))
            {
                _resettingRecDates = true;
                try
                {
                    RecStartDate = null;
                    RecEndDate = null;
                }
                finally
                {
                    _resettingRecDates = false;
                }
            }
            RecCurrentPage = 1;
            RefreshRec();
            ScheduleRecommendations();
        }

        partial void OnRecStartDateChanged(DateTime? value)
        {
            if (!_resettingRecDates && (value.HasValue || RecEndDate.HasValue)) SelectedRecWindow = "custom";
            RecCurrentPage = 1;
            RefreshRec();
            ScheduleRecommendations();
        }

        partial void OnRecEndDateChanged(DateTime? value)
        {
            if (!_resettingRecDates && (value.HasValue || RecStartDate.HasValue)) SelectedRecWindow = "custom";
            RecCurrentPage = 1;
            RefreshRec();
            ScheduleRecommendations();
        }

        partial void OnRecCurrentPageChanged(int value)
        {
            OnPropertyChanged(nameof(CanPreviousRecPage));
            OnPropertyChanged(nameof(CanNextRecPage));
        }

        partial void OnRecTotalPagesChanged(int value)
        {
            OnPropertyChanged(nameof(CanPreviousRecPage));
            OnPropertyChanged(nameof(CanNextRecPage));
        }

        [RelayCommand]
        private void PreviousRecPage()
        {
            if (!CanPreviousRecPage) return;
            RecCurrentPage--;
            RefreshRec();
        }

        [RelayCommand]
        private void NextRecPage()
        {
            if (!CanNextRecPage) return;
            RecCurrentPage++;
            RefreshRec();
        }

        /// <summary>方向/范围变化 → 重过滤 + 重算统计。</summary>
        private void RefreshRec()
        {
            RecView?.Refresh();
            RecomputeRecStats();
        }

        private bool FilterRec(object o)
        {
            var r = o as RecommendationRow;
            if (r == null) return false;
            if (r.Side != SelectedRecSide) return false;
            if (SelectedRecScope != RecommendationScope.Combined && r.Scope != SelectedRecScope) return false;
            return true;
        }

        private void RecomputeRecStats()
        {
            RecBidCount = Recommendations.Count(r => r.Side == QuoteDirection.Bid);
            RecOfrCount = Recommendations.Count(r => r.Side == QuoteDirection.Ofr);
            var filtered = RecView != null
                ? RecView.Cast<RecommendationRow>().ToList()
                : new System.Collections.Generic.List<RecommendationRow>();

            HasRecommendations = filtered.Count > 0;
            RecTotalPages = Math.Max(1, (int)Math.Ceiling(filtered.Count / (double)RecPageSize));
            if (RecCurrentPage > RecTotalPages) RecCurrentPage = RecTotalPages;
            if (RecCurrentPage < 1) RecCurrentPage = 1;

            RecPageItems.Clear();
            foreach (var row in filtered.Skip((RecCurrentPage - 1) * RecPageSize).Take(RecPageSize))
                RecPageItems.Add(row);

            RecPageSummary = string.Format("共{0}条 · 第{1}/{2}页", filtered.Count, RecCurrentPage, RecTotalPages);
            OnPropertyChanged(nameof(CanPreviousRecPage));
            OnPropertyChanged(nameof(CanNextRecPage));
        }

        #endregion

        // ============ 宿主行情订阅灌入（IHostQuoteFeed 回调，行情区块唯一数据源） ============

        private string _nzYtmText;
        private string _nzNetText;

        /// <summary>档位来源单元格：展示 Venues 短标签（国利/信唐/我…），缺省回退机构名。</summary>
        private static string SourceCell(QuoteFeedLevel level)
        {
            if (level == null) return string.Empty;
            return string.IsNullOrEmpty(level.SourceLabel) ? level.Institution ?? string.Empty : level.SourceLabel;
        }

        /// <summary>NzDisplay 分段维护：YTM / 净价 两段独立更新（null = 保持原段）。</summary>
        private void SetNzParts(string ytm, string net)
        {
            if (ytm != null) _nzYtmText = ytm;
            if (net != null) _nzNetText = net;
            NzDisplay = string.Format("{0} / {1}", _nzYtmText ?? "—", _nzNetText ?? "—");
        }

        /// <summary>宿主 CT 快照：Bid/Ofr 分侧按档位排序后按行配对，覆盖多档表与最优报价条。</summary>
        public void ApplyFeedLevels(QuoteFeedLevel[] levels)
        {
            if (!HasDetail || levels == null) return;
            // Bid/Ofr 是两条独立报价流：分侧排序缓存，表格行 = 当前筛选下两侧按序拉链
            _bidLevels = levels.Where(l => l.Side == QuoteFeedSide.Bid).OrderByDescending(l => l.Price).ToList();
            _ofrLevels = levels.Where(l => l.Side == QuoteFeedSide.Ofr).OrderBy(l => l.Price).ToList();
            // 空快照表示盘口清空，必须移除旧档位和最优价。
            RefreshTapeRows();
            // 盘口价格不是中债估值，估值仅由行内参考数据及债券主数据更新。
        }

        /// <summary>宿主最新成交：覆盖最新价与时间。</summary>
        public void ApplyFeedLastExec(QuoteFeedLastExec exec)
        {
            if (!HasDetail || exec == null || (!exec.Price.HasValue && !exec.Yield.HasValue)) return;
            LastPrice = exec.Price;
            LastYield = exec.Yield;
            LastExecDisplay = exec.Yield.HasValue
                ? exec.Yield.Value.ToString("0.0000") + "%"
                : exec.Price.Value.ToString("0.0000");
            if (exec.Time.HasValue) QuoteTimeDisplay = exec.Time.Value.ToString("HH:mm");
            OnPropertyChanged(nameof(IsUp));
        }

        /// <summary>宿主债券主数据：简称/剩余期限/中债估值收益率（仅覆盖有值字段）。</summary>
        public void ApplyFeedBondInfo(QuoteFeedBondInfo info)
        {
            if (!HasDetail || info == null || info.Code != Code) return;
            if (!string.IsNullOrEmpty(info.ShortName)) Name = info.ShortName;
            if (!string.IsNullOrEmpty(info.RemainingTime)) Maturity = info.RemainingTime;
            if (!string.IsNullOrEmpty(info.ValuationShow)) SetNzParts(info.ValuationShow, null);
        }
    }
}
