using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 模糊报价页 VM：自由文本 → 逐行识别（伪条件）→ 左栏行点击联动右栏找券（分页）→ 确认转模糊报价。
    /// </summary>
    public partial class FuzzyQuoteViewModel : ObservableObject, IDisposable
    {
        /// <summary>示例文本（原型 _FUZZY_EXAMPLES）。</summary>
        private const string ExampleText =
            "收 城投 AAA 3到5Y 上市 公募\n" +
            "bid 短融 中票 央企 有担保 煤炭\n" +
            "出 银行间 银行二级 商金债 2025\n" +
            "ofr 非上市 私募 含权 赎回权 化工\n" +
            "收 大于20Y 高收益 投资级 AAA+ 省级";

        private const int PageSize = 50;

        private readonly IQuoteService _quoteService;
        private readonly List<FuzzyCandidate> _candidatesAll = new List<FuzzyCandidate>();
        private readonly LatestRequestCoordinator _candidateCoordinator = new LatestRequestCoordinator();

        public FuzzyQuoteViewModel(IQuoteService quoteService)
        {
            _quoteService = quoteService;

            _status.Add("title", "模糊识别结果")
                   .Add("count", "共 0 条");

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
        }

        #region 识别结果（左栏）

        public ObservableCollection<FuzzyParseRowViewModel> Rows { get; } = new ObservableCollection<FuzzyParseRowViewModel>();

        /// <summary>左栏活动行（点击 → 右栏联动查询候选）。</summary>
        [ObservableProperty]
        private FuzzyParseRowViewModel _activeRow;

        [ObservableProperty]
        private int _selectedCount;

        [ObservableProperty]
        private int _totalCount;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        partial void OnActiveRowChanged(FuzzyParseRowViewModel value)
        {
            if (value == null) return;
            _ = QueryCandidatesAsync(value.Content);
        }

        partial void OnStatusMessageChanged(string value)
        {
            _status.Set("title", string.IsNullOrEmpty(value) ? "模糊识别结果" : value);
        }

        private void RecomputeStats()
        {
            SelectedCount = Rows.Count(r => r.IsSelected);
            TotalCount = Rows.Count;
            _status.Set("count", string.Format("共 {0} 条", Rows.Count));
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            Rows.Clear();
            Candidates.Clear();
            _candidatesAll.Clear();
            RecomputeStats();
            RecomputePagination();
        }

        #endregion

        #region 输入区

        [ObservableProperty]
        private string _inputText = string.Empty;

        [ObservableProperty]
        private bool _keepTextAfterParse;

        [ObservableProperty]
        private bool _isParsing;

        [RelayCommand]
        private void Paste()
        {
            try { InputText = Clipboard.GetText(); }
            catch (Exception) { ToastService.Show("读取剪贴板失败", ToastKind.Error); }
        }

        [RelayCommand]
        private void FillExample()
        {
            InputText = ExampleText;
        }

        [RelayCommand]
        private void ClearInput()
        {
            InputText = string.Empty;
        }

        [RelayCommand]
        private void DoSystemScreenshot()
        {
            ToastService.Show("截图识别待接桌面截屏 + OCR 服务");
        }

        [RelayCommand]
        private void ImportImage()
        {
            ToastService.Show("图片识别待接 OCR 服务");
        }

        [RelayCommand]
        private void ImportTable()
        {
            ToastService.Show("表格导入待接 xlsx/csv 解析");
        }

        [RelayCommand]
        private void Identify()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                ToastService.Show("请先粘贴报价文本", ToastKind.Warning);
                return;
            }
            if (IsParsing) return;

            IsParsing = true;
            try
            {
                Rows.Clear();
                foreach (var line in InputText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var text = line.Trim();
                    if (text.Length == 0) continue;
                    Rows.Add(new FuzzyParseRowViewModel("fi-" + Guid.NewGuid().ToString("N").Substring(0, 8), text)
                    {
                        ConditionsLabel = BuildPseudoConditions(text),
                    });
                }

                if (!KeepTextAfterParse) InputText = string.Empty;

                StatusMessage = string.Format("识别完成 · 共 {0} 条", Rows.Count);
                RecomputeStats();
                if (Rows.Count > 0) ActiveRow = Rows[0];
            }
            finally
            {
                IsParsing = false;
            }
        }

        /// <summary>Mock 伪条件提取（真实条件集由 intent-recognition 服务返回，接入时替换）。</summary>
        private static string BuildPseudoConditions(string text)
        {
            var parts = new List<string>();
            var upper = text.ToUpperInvariant();
            if (upper.Contains("BID") || upper.StartsWith("收")) parts.Add("方向 Bid");
            else if (upper.Contains("OFR") || upper.StartsWith("出")) parts.Add("方向 Ofr");
            if (upper.Contains("AAA")) parts.Add("评级 AAA");
            if (upper.Contains("城投")) parts.Add("城投");
            if (upper.Contains("中票")) parts.Add("券种 中票");
            if (upper.Contains("短融")) parts.Add("券种 短融");
            if (upper.Contains("3到5Y") || upper.Contains("3-5Y")) parts.Add("期限 3-5Y");
            if (upper.Contains("大于20Y")) parts.Add("期限 >20Y");
            if (upper.Contains("上市")) parts.Add("上市");
            if (upper.Contains("公募")) parts.Add("公募");
            if (upper.Contains("私募")) parts.Add("私募");
            if (parts.Count == 0) parts.Add("未识别出条件");
            return string.Join(" · ", parts);
        }

        #endregion

        #region 批量（左栏作用域）

        [ObservableProperty]
        private string _batchInstitution = string.Empty;

        [ObservableProperty]
        private string _batchTrader = string.Empty;

        partial void OnBatchInstitutionChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            foreach (var row in Rows.Where(r => r.IsSelected)) row.Institution = value.Trim();
        }

        partial void OnBatchTraderChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            foreach (var row in Rows.Where(r => r.IsSelected)) row.Trader = value.Trim();
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            var selected = Rows.Where(r => r.IsSelected).ToList();
            if (selected.Count == 0) { ToastService.Show("请先勾选要删除的行", ToastKind.Warning); return; }
            foreach (var row in selected) Rows.Remove(row);
            RecomputeStats();
        }

        /// <summary>确认报价：勾选行转模糊报价（原型 fuzzyConfirmToIntent → POST /api/board-intents/bulk）。</summary>
        [RelayCommand]
        private async Task ConfirmToIntent()
        {
            var selected = Rows.Where(r => r.IsSelected).ToList();
            if (selected.Count == 0) { ToastService.Show("请先勾选要确认的行", ToastKind.Warning); return; }

            var now = DateTime.Now;
            var intents = selected.Select(row => new BoardIntentRow
            {
                IntentId = "mock-intent-" + row.ClientItemId,
                Content = row.Content,
                RecognizedConditions = row.ConditionsLabel,
                Institution = row.Institution,
                Trader = row.Trader,
                WallTrader = "当前用户",
                QuoteTime = now,
                Status = IntentStatus.Active,
                IsPendingSync = false,
            }).ToList();

            try
            {
                await _quoteService.SubmitIntentQuotesAsync(intents, CancellationToken.None);
                foreach (var row in selected) Rows.Remove(row);
                RecomputeStats();
                ToastService.Show(string.Format("已转 {0} 条模糊报价", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("转模糊报价失败：" + ex.Message, ToastKind.Error);
            }
        }

        #endregion

        #region 右栏候选（分页）

        public ObservableCollection<FuzzyCandidate> Candidates { get; } = new ObservableCollection<FuzzyCandidate>();

        [ObservableProperty]
        private int _candidateTotal;

        [ObservableProperty]
        private int _currentPage = 1;

        [ObservableProperty]
        private int _totalPages = 1;

        private async Task QueryCandidatesAsync(string text)
        {
            var lease = _candidateCoordinator.Begin();
            try
            {
                var all = await _quoteService.QueryFuzzyCandidatesAsync(
                    new FuzzyQuery { Text = text, Limit = 1000 }, lease.Token);
                if (!lease.IsCurrent) return;
                _candidatesAll.Clear();
                _candidatesAll.AddRange(all);
                CurrentPage = 1;
                RecomputePagination();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                if (lease.IsCurrent) ToastService.Show("候选查询失败：" + ex.Message, ToastKind.Error);
            }
            finally
            {
                lease.Dispose();
            }
        }

        private void RecomputePagination()
        {
            CandidateTotal = _candidatesAll.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling(_candidatesAll.Count / (double)PageSize));

            Candidates.Clear();
            foreach (var item in _candidatesAll.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            {
                Candidates.Add(item);
            }
        }

        partial void OnCurrentPageChanged(int value)
        {
            RecomputePagination();
        }

        [RelayCommand]
        private void PrevPage()
        {
            if (CurrentPage > 1) CurrentPage--;
        }

        [RelayCommand]
        private void NextPage()
        {
            if (CurrentPage < TotalPages) CurrentPage++;
        }

        #endregion

        public void Dispose()
        {
            _candidateCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
        }
    }
}
