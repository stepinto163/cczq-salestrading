using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.Converters;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 精确报价页 VM：多行文本粘贴 → 识别为结构化行 → 18+1 列可编辑预览表 → 批量工具栏 → 确认入库。
    /// 输入区高度可拖拽（100–500px）；Enter 换行 / Ctrl+Enter 识别。
    /// </summary>
    public partial class PreciseQuoteViewModel : ObservableObject, IDisposable
    {
        /// <summary>示例文本（原型 _PRECISE_EXAMPLES）。</summary>
        private const string ExampleText =
            "bid 240210.IB 2.9155 2000 T+1\n" +
            "ref bid 240210.IB 2.9155 2000\n" +
            "252980 3.80/3.85 1000/500\n" +
            "refer 252980 3.80/3.85 1000/500\n" +
            "bid 102580247.IB 净价 100.125 3000 T+0\n" +
            "ref 102580247.IB\n" +
            "ofr 252980.SH 全价 105.680 1500 T+1\n" +
            "收 21柳发02 3.50 500\n" +
            "24国开10 bid 2.9155 出 ofr 2.92 2000\n" +
            "bid 210210.IB 净价 101.020 1000 T+1\n" +
            "ofr 240210.IB 全价 102.380 2000 T+0";

        private readonly IQuoteService _quoteService;
        private readonly HashSet<QuoteConfirmRowViewModel> _hooked = new HashSet<QuoteConfirmRowViewModel>();
        private readonly LatestRequestCoordinator _parseCoordinator = new LatestRequestCoordinator();

        public PreciseQuoteViewModel(IQuoteService quoteService)
        {
            _quoteService = quoteService;

            _status.Add("title", "精确识别结果")
                   .Add("count", "共 0 条")
                   .Add("ofr", "Ofr: 0", "OfrBrush", "#3B82F6")
                   .Add("bid", "Bid: 0", "BidBrush", "#F59E0B");

            EventAggregator.Subscribe<DataSourceResetMessage>(this, OnDataSourceReset);
        }

        #region 结果集合与统计

        public ObservableCollection<QuoteConfirmRowViewModel> Rows { get; } = new ObservableCollection<QuoteConfirmRowViewModel>();

        [ObservableProperty]
        private int _selectedCount;

        [ObservableProperty]
        private int _totalCount;

        /// <summary>识别状态（空 = 默认；非空时展示在状态栏首段，原型为浮动状态条）。</summary>
        [ObservableProperty]
        private string _statusMessage = string.Empty;

        private readonly StatusSectionBag _status = new StatusSectionBag();

        public ObservableCollection<StatusBarSection> StatusSections { get { return _status.Sections; } }

        private IEnumerable<QuoteConfirmRowViewModel> SelectedRows
        {
            get { return Rows.Where(r => r.IsSelected); }
        }

        private void RecomputeStats()
        {
            SelectedCount = Rows.Count(r => r.IsSelected);
            TotalCount = Rows.Count;
            var ofr = Rows.Count(r => r.Direction == "Ofr");
            var bid = Rows.Count(r => r.Direction == "Bid");

            _status.Set("title", string.IsNullOrEmpty(StatusMessage) ? "精确识别结果" : StatusMessage);
            _status.Set("count", string.Format("共 {0} 条", Rows.Count));
            _status.Set("ofr", "Ofr: " + ofr);
            _status.Set("bid", "Bid: " + bid);
        }

        partial void OnStatusMessageChanged(string value)
        {
            _status.Set("title", string.IsNullOrEmpty(value) ? "精确识别结果" : value);
        }

        private void OnDataSourceReset(DataSourceResetMessage message)
        {
            UnhookAll();
            Rows.Clear();
            RecomputeStats();
        }

        #endregion

        #region 输入区

        [ObservableProperty]
        private string _inputText = string.Empty;

        /// <summary>输入区高度（拖拽手柄调整，100–500px，原型 preciseResizeHandle）。</summary>
        [ObservableProperty]
        private double _inputHeight = 200;

        [ObservableProperty]
        private bool _keepTextAfterParse;

        [ObservableProperty]
        private bool _isParsing;

        partial void OnInputHeightChanged(double value)
        {
            if (value < 100) InputHeight = 100;
            if (value > 500) InputHeight = 500;
        }

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
            ToastService.Show("截图识别待接桌面截屏 + OCR 服务（原型 Tesseract）");
        }

        [RelayCommand]
        private void ImportImage()
        {
            ToastService.Show("图片识别待接 OCR 服务（原型 Tesseract）");
        }

        [RelayCommand]
        private void ImportTable()
        {
            ToastService.Show("表格导入待接 xlsx/csv 解析（原型 SheetJS）");
        }

        #endregion

        #region 识别

        [RelayCommand]
        private async Task IdentifyAsync()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                ToastService.Show("请先粘贴报价文本", ToastKind.Warning);
                return;
            }
            if (IsParsing) return;

            IsParsing = true;
            var lease = _parseCoordinator.Begin();
            try
            {
                var result = await _quoteService.ParsePreciseTextAsync(InputText, lease.Token);
                if (!lease.IsCurrent) return;

                UnhookAll();
                Rows.Clear();
                foreach (var row in result.Rows)
                {
                    var rvm = new QuoteConfirmRowViewModel(row) { RowTypeLabel = "新增" };
                    if (_hooked.Add(rvm)) rvm.PropertyChanged += OnRowPropertyChanged;
                    Rows.Add(rvm);
                }

                if (!KeepTextAfterParse) InputText = string.Empty;

                StatusMessage = string.Format("识别完成 · 成功 {0} 条 · 失败 {1} 条 · 耗时 {2}ms",
                    result.SucceededCount, result.FailedCount, (int)result.Elapsed.TotalMilliseconds);
                if (result.Warnings != null && result.Warnings.Count > 0)
                {
                    // 首条提示（如"识别服务暂不可用，以下为本地简析结果"）弹泡提醒
                    ToastService.Show(result.Warnings[0], ToastKind.Warning);
                }
                RecomputeStats();
            }
            catch (Exception ex)
            {
                StatusMessage = "识别失败：" + ex.Message;
                RecomputeStats();
            }
            finally
            {
                lease.Dispose();
                IsParsing = false;
            }
        }

        #endregion

        #region 确认 / 删除（精确页作用域）

        [RelayCommand]
        private async Task ConfirmSelected()
        {
            var selected = SelectedRows.ToList();
            if (selected.Count == 0)
            {
                ToastService.Show("请先勾选要确认的报价", ToastKind.Warning); return;
            }

            foreach (var row in selected) row.Commit();
            try
            {
                await _quoteService.SubmitQuotesAsync(
                    selected.Select(row => row.Row).ToList(),
                    CancellationToken.None);
                ToastService.Show(string.Format("已确认 {0} 条报价", selected.Count), ToastKind.Success);
            }
            catch (Exception ex)
            {
                ToastService.Show("确认报价失败：" + ex.Message, ToastKind.Error);
            }
            finally
            {
                // 批次部分成功时，只移除确实 ACTIVE 的行；失败行留在预览中继续修正。
                RemoveRows(selected.Where(row => row.Row.Status == Abstractions.Models.QuoteStatus.Active).ToList());
            }
        }

        [RelayCommand]
        private void DeleteSelected()
        {
            var selected = SelectedRows.ToList();
            if (selected.Count == 0) { ToastService.Show("请先勾选要删除的报价", ToastKind.Warning); return; }

            RemoveRows(selected);
            ToastService.Show(string.Format("已删除 {0} 条报价", selected.Count));
        }

        private void RemoveRows(IEnumerable<QuoteConfirmRowViewModel> rows)
        {
            foreach (var row in rows.ToList())
            {
                if (_hooked.Remove(row)) row.PropertyChanged -= OnRowPropertyChanged;
                Rows.Remove(row);
            }
            RecomputeStats();
        }

        #endregion

        #region 批量工具栏（同报价确认页语义）

        [ObservableProperty]
        private string _bpInput = string.Empty;

        [ObservableProperty]
        private string _amountInput = string.Empty;

        [ObservableProperty]
        private string _institutionInput = string.Empty;

        /// <summary>机构批量框选中的 CRM Id（CrmSearchBox 回填；null=未选定）。</summary>
        [ObservableProperty]
        private string _institutionSelectedId;

        [ObservableProperty]
        private string _traderInput = string.Empty;

        /// <summary>交易员批量框选中的 CRM Id。</summary>
        [ObservableProperty]
        private string _traderSelectedId;

        [ObservableProperty]
        private string _toolbarPriceType = "到期收益率";

        partial void OnToolbarPriceTypeChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var row in SelectedRows) row.PriceType = value;
        }

        [ObservableProperty]
        private string _toolbarSettlement = "T+1";

        partial void OnToolbarSettlementChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var row in SelectedRows) row.Settlement = value;
        }

        [RelayCommand]
        private void SetDirectionBid()
        {
            foreach (var row in SelectedRows) row.Direction = "Bid";
        }

        [RelayCommand]
        private void SetDirectionOfr()
        {
            foreach (var row in SelectedRows) row.Direction = "Ofr";
        }

        [RelayCommand]
        private void AdjustBpUp() { ApplyBp(1); }

        [RelayCommand]
        private void AdjustBpDown() { ApplyBp(-1); }

        private void ApplyBp(int sign)
        {
            decimal bp;
            if (!decimal.TryParse(BpInput, out bp)) bp = 0;
            var delta = bp * sign;

            foreach (var row in SelectedRows)
            {
                decimal price;
                if (!decimal.TryParse(row.Price, out price)) continue;
                var step = row.PriceType == "净价" || row.PriceType == "全价" ? 0.01m : 0.0001m;
                row.Price = (price + delta * step).ToString("0.####", CultureInfo.InvariantCulture);
            }
        }

        [RelayCommand]
        private void AdjustAmountUp() { ApplyAmount(1); }

        [RelayCommand]
        private void AdjustAmountDown() { ApplyAmount(-1); }

        private void ApplyAmount(int sign)
        {
            decimal amount;
            if (!decimal.TryParse(AmountInput, out amount)) amount = 0;
            var delta = amount * sign * 1000m;

            foreach (var row in SelectedRows)
            {
                decimal vol;
                if (!decimal.TryParse(row.Volume, out vol)) vol = 0;
                row.Volume = Math.Max(0, vol + delta).ToString("0.#", CultureInfo.InvariantCulture);
            }
        }

        partial void OnInstitutionInputChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            foreach (var row in SelectedRows)
            {
                row.Institution = value.Trim();
                row.Row.InstitutionId = _institutionSelectedId;
            }
        }

        partial void OnInstitutionSelectedIdChanged(string value)
        {
            // 选中回填的 Text 变化先到，这里补写 Id（两者幂等）
            if (value == null) return;
            foreach (var row in SelectedRows) row.Row.InstitutionId = value;
        }

        partial void OnTraderInputChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            foreach (var row in SelectedRows)
            {
                row.Trader = value.Trim();
                row.Row.ContactId = _traderSelectedId;
            }
        }

        partial void OnTraderSelectedIdChanged(string value)
        {
            if (value == null) return;
            foreach (var row in SelectedRows) row.Row.ContactId = value;
        }

        [RelayCommand]
        private void ToggleFlag(string flag)
        {
            foreach (var row in SelectedRows)
            {
                switch (flag)
                {
                    case "oco": row.IsOco = !row.IsOco; break;
                    case "packing": row.IsPacking = !row.IsPacking; break;
                    case "urgent": row.IsUrgent = !row.IsUrgent; break;
                    case "rec": row.IsRec = !row.IsRec; break;
                    case "whole": row.IsWhole = !row.IsWhole; break;
                }
            }
        }

        #endregion

        #region 行钩子

        private void OnRowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(QuoteConfirmRowViewModel.IsSelected))
            {
                RecomputeStats();
            }
        }

        private void UnhookAll()
        {
            foreach (var row in _hooked) row.PropertyChanged -= OnRowPropertyChanged;
            _hooked.Clear();
        }

        public void Dispose()
        {
            _parseCoordinator.Dispose();
            EventAggregator.Unsubscribe(this);
            UnhookAll();
        }

        #endregion
    }
}
