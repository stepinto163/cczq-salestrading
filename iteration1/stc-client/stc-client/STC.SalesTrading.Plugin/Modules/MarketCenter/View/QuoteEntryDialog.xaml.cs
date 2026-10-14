using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 已有报价编辑弹窗（原型 0910「编辑报价」，结构 = 遮罩 rgba(0,0,0,.6) + .modal.modal-wide 880px）：
    /// 债券摘要条（市场最优 Bid｜Ofr + 中债估值）→ 方向分段 → 价格（±1bp）/报量（±1k，含单位下拉）→
    /// 交易日期/清算速度/报价机构/报价交易员 → 标签（急/荐/整）+ 备注 → 页脚 Enter 保存修改 / Esc 取消。
    ///
    /// 遮罩说明：原型遮罩是覆盖整个应用视口的固定层（WPF 独立 Window 无法给宿主叠加半透明层），
    /// 因此本窗口在 OnLoaded 时铺满宿主窗口（<see cref="OverlayBackdrop"/>），内部用半透明黑 Border 做遮罩。
    ///
    /// 真实报价编辑走 QuoteSvr；新增报价统一转到精确报价，禁止回退到手工创建表单。
    /// </summary>
    public partial class QuoteEntryDialog : Window
    {
        /// <summary>价格步进（原型 adjustQuoteInput('quotePrice', 0.01, 4)）。</summary>
        private const decimal PriceStep = 0.01m;

        /// <summary>报量步进，单位万（原型 adjustQuoteInput('quoteVolume', 1000, 0)）。</summary>
        private const decimal VolumeStep = 1000m;

        /// <summary>弹窗四周与宿主边界的最小留白（原型正文 max-height 70vh + 头尾）。</summary>
        private const double ShellMargin = 40d;

        private QuoteEntryDialog()
        {
            InitializeComponent();
            BestBidText.Text = "--";
            BestOfrText.Text = "--";
            CbValuationText.Text = "—";
            ClearValidation();
            Loaded += OnLoaded;
        }

        /// <summary>铺满宿主并按其高度限制弹窗最大高度（原型正文 max-height:70vh，超出滚动）。</summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplyHostGeometry();
        }

        /// <summary>对齐宿主尺寸与弹窗最大高度；在 ShowDialog 之前调用可避免窗口先出现在默认位置再跳动。</summary>
        private void ApplyHostGeometry()
        {
            try
            {
                OverlayBackdrop.CoverOwner(this);
                var host = OverlayBackdrop.OwnerSize(this);
                ModalShell.MaxHeight = Math.Max(360d, host.Height - ShellMargin);
            }
            catch (Exception)
            {
                // 尺寸收敛失败不影响弹窗可用性，保留 XAML 基准值。
            }
        }

        /// <summary>从看板行预填并打开（模态）。任何失败以 toast 呈现，避免"双击没反应"。</summary>
        public static void ShowForRow(Window owner, BoardQuoteRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            if (row != null && Ficc.DpsUiContext.OpenQuote(owner, mode == QuoteFormMode.New ? null : row.QuoteId, row.Code))
            {
                return;
            }
            ToastService.Show("报价服务未就绪，无法打开报价操作。", ToastKind.Warning);
        }

        public static void ShowForRow(Window owner, BoardBestRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            if (mode == QuoteFormMode.Edit && (row?.SourceQuoteIds == null || row.SourceQuoteIds.Length != 1))
            {
                ToastService.Show("请到全部报价中选择具体一条报价后修改。", ToastKind.Warning);
                return;
            }
            if (row != null && Ficc.DpsUiContext.OpenQuote(owner, mode == QuoteFormMode.New ? null : row.SourceQuoteIds[0], row.Code))
            {
                return;
            }
            ToastService.Show("报价服务未就绪，无法打开报价操作。", ToastKind.Warning);
        }

        /// <summary>提醒行也能发起报价（提醒页右键菜单与看板共用一套）。</summary>
        public static void ShowForRow(Window owner, BoardAlertRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            if (mode == QuoteFormMode.Edit)
            {
                ToastService.Show("提醒不是单笔报价，请在报价管理中选择报价。", ToastKind.Warning);
                return;
            }
            if (row != null && Ficc.DpsUiContext.OpenQuote(owner, null, row.Code))
            {
                return;
            }
            ToastService.Show("报价服务未就绪，无法打开报价操作。", ToastKind.Warning);
        }

        /// <summary>摘要条：债券名（13px 粗）+ 代码（16px 粗）+ 三项 meta（剩余期限/主体评级/债项评级）。</summary>
        private void SetBondHeader(string code, string name, string maturity, string bondLibrary,
            string issuerRating, string bondRating)
        {
            BondCode.Text = code ?? string.Empty;
            BondName.Text = name ?? string.Empty;
            MaturityMeta.Text = "剩余期限 " + Dash(maturity);
            IssuerRatingMeta.Text = "主体评级 " + Dash(issuerRating);
            BondRatingMeta.Text = "债项评级 " + Dash(bondRating);

            // 原型摘要条 meta 只有三项，债券库不在其中；挂到弹窗 ToolTip 备查，避免信息丢失
            ModalShell.ToolTip = string.IsNullOrEmpty(bondLibrary) ? null : "债券库 " + bondLibrary;
        }

        private static string Dash(string value)
        {
            return string.IsNullOrEmpty(value) ? "—" : value;
        }

        #region 价格 / 报量步进

        private void OnPriceStepUp(object sender, RoutedEventArgs e) { Step(PriceBox, PriceStep, "0.0000"); }

        private void OnPriceStepDown(object sender, RoutedEventArgs e) { Step(PriceBox, -PriceStep, "0.0000"); }

        private void OnVolumeStepUp(object sender, RoutedEventArgs e) { Step(VolumeBox, VolumeStep, "0.#"); }

        private void OnVolumeStepDown(object sender, RoutedEventArgs e) { Step(VolumeBox, -VolumeStep, "0.#"); }

        /// <summary>步进一次：空值按 0 起算，结果不允许为负（原型 input min=0）。</summary>
        private static void Step(TextBox box, decimal delta, string format)
        {
            if (box == null) return;

            decimal current;
            if (!decimal.TryParse(box.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out current))
            {
                current = 0m;
            }

            var next = current + delta;
            if (next < 0m) next = 0m;
            box.Text = next.ToString(format, CultureInfo.InvariantCulture);
        }

        #endregion

        #region 关闭 / 标题栏

        /// <summary>遮罩空白处点击关闭（原型 onclick="if(event.target===this)closeModal()"）。</summary>
        private void OnBackdropMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (busy) return;
            Close();
        }

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (Exception) { }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            if (busy) return;
            Close();
        }

        /// <summary>原型 kbd 提示：Enter 保存修改、Esc 取消（备注多行输入不拦截 Enter）。</summary>
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (busy) return;
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
                return;
            }

            if (e.Key != Key.Enter) return;

            var focused = Keyboard.FocusedElement as TextBox;
            if (focused != null && focused.AcceptsReturn) return;

            OnSubmit(sender, e);
            e.Handled = true;
        }

        #endregion

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (busy) return;
            Close();
        }

        /// <summary>校验后提交（原型 validateQuoteForm：价格/报量非空须为不小于 0 的有效数字）。</summary>
        private async void OnSubmit(object sender, RoutedEventArgs e)
        {
            if (liveApi == null || originalQuote == null)
            {
                SubmitError.Text = "只能修改已存在的报价；新增请使用精确报价或接入报价。";
                return;
            }
            await SaveLiveQuoteAsync();
        }

        #region 校验态（原型 .quote-control.invalid{border-color:#f85149}）

        private void ClearValidation()
        {
            PriceErrorText.Text = string.Empty;
            VolumeErrorText.Text = string.Empty;
            PriceBox.ClearValue(BorderBrushProperty);
            VolumeBox.ClearValue(BorderBrushProperty);
        }

        private void MarkInvalid(TextBox box, TextBlock error, string message)
        {
            error.Text = message;
            var danger = TryFindResource("DangerBrush") as Brush;
            if (danger != null)
            {
                box.BorderBrush = danger;
            }

            box.Focus();
        }

        #endregion
    }
}
