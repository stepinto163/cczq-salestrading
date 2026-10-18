using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Abstractions.Models;
using STC.SalesTrading.Plugin.Shared.Services;
using STC.SalesTrading.Plugin.Shared.ViewModel;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>
    /// 报价表单弹窗（原型 0910「新报价 / 编辑报价」，普通模态窗口 880×620，居中于宿主）：
    /// 债券摘要条（市场最优 Bid｜Ofr + 中债估值）→ 方向分段 → 价格（±1bp）/报量（±1k，含单位下拉）→
    /// 交易日期/清算速度/报价机构/报价交易员 → 标签（急/荐/整）+ 备注 → 页脚 Enter 保存修改 / Esc 取消。
    ///
    /// 从看板行预填；提交经 <see cref="QuoteService"/>（组合根注入）：编辑行带报价 Id 走 update、
    /// 其余走 create（Gateway 实现 ficc.quote.create/update）。
    /// </summary>
    public partial class QuoteEntryDialog : Window
    {
        /// <summary>价格步进（原型 adjustQuoteInput('quotePrice', 0.01, 4)）。</summary>
        private const decimal PriceStep = 0.01m;

        /// <summary>报量步进，单位万（原型 adjustQuoteInput('quoteVolume', 1000, 0)）。</summary>
        private const decimal VolumeStep = 1000m;

        /// <summary>要编辑的报价 Id。null 表示新建。</summary>
        private string editQuoteId;

        /// <summary>要编辑报价的版本，提交时作 expectedRevision。</summary>
        private string editRevision;

        /// <summary>
        /// 录入服务（组合根装配时注入）。新报价走 create、编辑走 update；
        /// 未注入（异常装配路径）时提交提示服务未就绪。
        /// </summary>
        public static IQuoteService QuoteService { get; set; }

        /// <summary>tab 关闭时解除静态服务引用（仅当归属本 tab）：关闭已打开的窗口并清空 QuoteService。
        /// 服务集 Dispose 后窗口若仍开着，提交会走已释放的流/视图（半死状态）；静态不清则整套服务图被钉住不回收。</summary>
        public static void DetachServices(IQuoteService expected)
        {
            if (!ReferenceEquals(QuoteService, expected)) return;
            QuoteService = null;
            _active?.Close();   // Closed 里已清 _active；未打开时无操作
        }

        public QuoteEntryDialog()
        {
            InitializeComponent();
            BestBidText.Text = "--";
            BestOfrText.Text = "--";
            CbValuationText.Text = "—";
            ClearValidation();
        }

        /// <summary>从看板行预填并打开（非模态、全局唯一：已打开时激活置前）。任何失败以 toast 呈现，避免"双击没反应"。
        /// 编辑模式且行关联报价时，先按行预填再异步拉完整报价覆盖（方案 §3.1.4 专项 1）。</summary>
        private static QuoteEntryDialog _active;
        public static void ShowForRow(Window owner, TapeRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            ShowCore(owner, mode, dlg => dlg.Prefill(row, mode), mode == QuoteFormMode.Edit ? row?.QuoteId : null);
        }
        public static void ShowForRow(Window owner, BestRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            // 聚合行取首个关联报价拉全量（无关联则按行预填）
            var pullId = mode == QuoteFormMode.Edit && row?.SourceQuoteIds != null && row.SourceQuoteIds.Length > 0
                ? row.SourceQuoteIds[0] : null;
            ShowCore(owner, mode, dlg => dlg.Prefill(row, mode), pullId);
        }

        /// <summary>提醒行也能发起报价（提醒页右键菜单与看板共用一套）。
        /// 新报价同样拉关联报价详情：提醒记录不带机构/交易员，按 QuoteIds[0] 取回报价即含 CRM Id，
        /// 避免仅名称无法确认生效（方案 §3.1.4 专项 2）。</summary>
        public static void ShowForRow(Window owner, AlertRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            var pullId = row?.QuoteIds != null && row.QuoteIds.Count > 0 ? row.QuoteIds[0] : null;
            ShowCore(owner, mode, dlg => dlg.Prefill(row, mode), pullId);
        }

        /// <summary>
        /// 从完整报价行预填并打开（报价管理今日页双击编辑，方案 §3.1.4 入口矩阵）：
        /// 行即 quote view 产出的完整 Quote（含 Revision），直接回填无需再拉。
        /// </summary>
        public static void ShowForQuote(Window owner, QuoteRow row, QuoteFormMode mode = QuoteFormMode.Edit)
        {
            ShowCore(owner, mode, dlg => dlg.Prefill(row, mode), null);
        }

        private static async void ShowCore(Window owner, QuoteFormMode mode, Action<QuoteEntryDialog> prefill, string pullQuoteId)
        {
            try
            {
                // 全局唯一：已打开时激活置前，不重复开（新预填内容丢弃）
                if (_active != null)
                {
                    if (_active.WindowState == WindowState.Minimized) _active.WindowState = WindowState.Normal;
                    _active.Activate();
                    return;
                }
                var dlg = new QuoteEntryDialog();
                var host = owner ?? Application.Current?.MainWindow;
                if (host != null) dlg.Owner = host;
                dlg.FormTitle.Text = mode == QuoteFormMode.New ? "新报价" : "编辑报价";
                prefill(dlg);

                // 非模态：await 前先占坑并显示窗口（拉取期间用户可看可操作主界面）
                _active = dlg;
                dlg.Closed += (s2, e2) =>
                {
                    if (ReferenceEquals(_active, dlg)) _active = null;
                };
                dlg.Show();

                // 方案 §3.1.4 专项 1：编辑时先拉完整报价回填（价格簿行不带备注/交易日期/旗标/yieldType；
                // 缺这些字段提交 update 会按"未传保留/null 清除"语义出错——尤其 remark 显式 null 会清空原值）。
                // 拉取失败降级为行数据预填继续（乐观锁用行上 revision），toast 提示。
                if (!string.IsNullOrEmpty(pullQuoteId) && QuoteService != null)
                {
                    try
                    {
                        if (!dlg.IsLoaded) return;      // 等待拉取期间用户已关闭窗口
                        dlg.FormTitle.Text += "（加载中…）";
                        var full = await QuoteService.GetQuoteAsync(pullQuoteId, CancellationToken.None);
                        if (full != null && dlg.IsLoaded) dlg.Prefill(full, mode);
                    }
                    catch (Exception pullError)
                    {
                        ToastService.Show("完整报价拉取失败，按行情行预填提交：" + pullError.Message, ToastKind.Warning);
                    }
                    if (dlg.IsLoaded) dlg.FormTitle.Text = mode == QuoteFormMode.New ? "新报价" : "编辑报价";
                }
            }
            catch (Exception ex)
            {
                ToastService.Show("打开报价录入失败：" + ex.Message, ToastKind.Error);
            }
        }

        #region 预填

        private void Prefill(TapeRow row, QuoteFormMode mode)
        {
            if (row == null) return;

            // 编辑模式锁定原报价（价格簿行不带备注/旗标等，提交前由服务层补拉完整报价）。
            if (mode == QuoteFormMode.Edit)
            {
                editQuoteId = row.QuoteId;
                editRevision = row.Revision;
            }

            SetBondHeader(row.SecurityId, row.SecurityName, row.Maturity, row.BondLibrary, row.IssuerRating, row.BondRating);

            // 单笔行情没有「市场最优」口径，留空显示 --；中债估值取该行的中债 YTM
            CbValuationText.Text = FormatValuation(row.NzYtm);

            if (row.Side == QuoteDirection.Bid) DirBid.IsChecked = true;
            else if (row.Side == QuoteDirection.Ofr) DirOfr.IsChecked = true;

            PriceBox.Text = row.Price?.ToString("0.0000", CultureInfo.InvariantCulture) ?? string.Empty;
            VolumeBox.Text = row.Quantity?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty;
            InstitutionBox.Text = row.InstitutionName ?? string.Empty;
            if (row.InstitutionId != null) InstitutionBox.SelectedValue = row.InstitutionId;
            TraderBox.Text = row.ContactName ?? string.Empty;
            if (row.ContactId != null) TraderBox.SelectedValue = row.ContactId;
            TradeDateBox.Text = FormatDate(row.UpdateTime ?? DateTime.Now);
            RemarkBox.Text = string.Format("来自看板行 {0} · 原报价 {1} {2}",
                row.SecurityId, row.Side, row.Price?.ToString("0.0000", CultureInfo.InvariantCulture));

            // 价格簿条目不携带报价类型：价格类型默认收益率口径
            SelectPriceType(PriceType.YieldToMaturity);
            SelectSettlement(row.SettlType);

            FlagUrgent.IsChecked = false;
            FlagRec.IsChecked = false;
            FlagWhole.IsChecked = false;

            ApplyModeDefaults(mode);
        }

        /// <summary>
        /// 完整报价预填（ficc.quote.get 结果，方案 §3.1.4 专项 1）：
        /// 覆盖行预填缺的备注/交易日期/旗标/yieldType，并回填 CRM Id（联想框显示标准名称）。
        /// </summary>
        private void Prefill(QuoteRow row, QuoteFormMode mode)
        {
            if (row == null) return;

            editQuoteId = row.QuoteId;
            editRevision = row.Revision;

            SetBondHeader(row.SecurityId, row.SecurityName, row.Maturity, row.BondLibrary, row.IssuerRating, row.BondRating);
            CbValuationText.Text = FormatValuation(row.NzYtm);

            if (row.Side == QuoteDirection.Bid) DirBid.IsChecked = true;
            else if (row.Side == QuoteDirection.Ofr) DirOfr.IsChecked = true;

            PriceBox.Text = row.Price?.ToString("0.0000", CultureInfo.InvariantCulture) ?? string.Empty;
            VolumeBox.Text = row.Quantity?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty;
            SelectPriceType(row.QuoteType);
            SelectSettlement(row.SettlType);
            TradeDateBox.Text = FormatDate(row.TradeDate ?? DateTime.Now);
            InstitutionBox.Text = row.InstitutionName ?? string.Empty;
            InstitutionBox.SelectedValue = row.InstitutionId;
            TraderBox.Text = row.ContactName ?? string.Empty;
            TraderBox.SelectedValue = row.ContactId;
            RemarkBox.Text = row.Remark ?? string.Empty;

            FlagUrgent.IsChecked = row.Urgent;
            FlagRec.IsChecked = row.Recommended;
            FlagWhole.IsChecked = row.Whole;

            // 完整报价已按行编辑语义覆盖，New 模式默认值不再叠加
            if (mode != QuoteFormMode.New) return;
            ApplyModeDefaults(mode);
        }

        private void Prefill(BestRow row, QuoteFormMode mode)
        {
            if (row == null) return;

            SetBondHeader(row.SecurityId, row.SecurityName, row.Maturity, row.BondLibrary, row.IssuerRating, row.BondRating);

            BestBidText.Text = row.Bid?.ToString("0.0000", CultureInfo.InvariantCulture) ?? "--";
            BestOfrText.Text = row.Ofr?.ToString("0.0000", CultureInfo.InvariantCulture) ?? "--";
            CbValuationText.Text = FormatValuation(row.NzYtm);

            PriceBox.Text = row.Ofr?.ToString("0.0000", CultureInfo.InvariantCulture)
                ?? row.Bid?.ToString("0.0000", CultureInfo.InvariantCulture) ?? string.Empty;
            VolumeBox.Text = row.OfrVolume?.ToString("0.#", CultureInfo.InvariantCulture)
                ?? row.BidVolume?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty;
            if (row.Ofr.HasValue) DirOfr.IsChecked = true;
            else if (row.Bid.HasValue) DirBid.IsChecked = true;

            // 机构/联系人取自同档明细（非本人报价为空，同浮层口径）
            var ofrDetail = row.OfrQuotes != null && row.OfrQuotes.Count > 0 ? row.OfrQuotes[0] : null;
            var bidDetail = row.BidQuotes != null && row.BidQuotes.Count > 0 ? row.BidQuotes[0] : null;
            InstitutionBox.Text = ofrDetail?.Institution ?? bidDetail?.Institution ?? string.Empty;
            TraderBox.Text = ofrDetail?.Trader ?? bidDetail?.Trader ?? string.Empty;
            TradeDateBox.Text = FormatDate(row.UpdateTime ?? DateTime.Now);
            RemarkBox.Text = string.Format("来自最优看板 {0} · Bid {1} / Ofr {2}",
                row.SecurityId,
                row.Bid?.ToString("0.0000", CultureInfo.InvariantCulture),
                row.Ofr?.ToString("0.0000", CultureInfo.InvariantCulture));

            // 最优行同样不携带报价类型/清算速度：按默认值（到期收益率 / T+1）处理
            SelectPriceType(PriceType.YieldToMaturity);
            SelectSettlement(SettlementType.T1);

            ApplyModeDefaults(mode);
        }

        /// <summary>提醒行的预填：提醒行不含价格类型/清算速度，按默认值（到期收益率 / T+1）处理。</summary>
        private void Prefill(AlertRow row, QuoteFormMode mode)
        {
            if (row == null) return;

            SetBondHeader(row.SecurityId, row.SecurityName, row.Maturity, row.BondLibrary, row.IssuerRating, row.BondRating);

            BestBidText.Text = row.Bid?.ToString("0.0000", CultureInfo.InvariantCulture) ?? "--";
            BestOfrText.Text = row.Ofr?.ToString("0.0000", CultureInfo.InvariantCulture) ?? "--";
            CbValuationText.Text = FormatValuation(row.NzYtm);

            PriceBox.Text = row.Ofr?.ToString("0.0000", CultureInfo.InvariantCulture)
                ?? row.Bid?.ToString("0.0000", CultureInfo.InvariantCulture) ?? string.Empty;
            VolumeBox.Text = row.VolOfr?.ToString("0.#", CultureInfo.InvariantCulture)
                ?? row.VolBid?.ToString("0.#", CultureInfo.InvariantCulture) ?? string.Empty;
            if (row.Ofr.HasValue) DirOfr.IsChecked = true;
            else if (row.Bid.HasValue) DirBid.IsChecked = true;

            InstitutionBox.Text = row.InstitutionName ?? string.Empty;
            TraderBox.Text = row.ContactName ?? string.Empty;
            TradeDateBox.Text = FormatDate(row.UpdateTime ?? DateTime.Now);

            SelectPriceType(PriceType.YieldToMaturity);
            SelectSettlement(SettlementType.T1);

            ApplyModeDefaults(mode);
        }

        /// <summary>
        /// 新报价（原型 showNewQuoteModal）：保留债券与价格预填，清掉备注与标签；
        /// 编辑报价（showEditQuoteModal）则完整保留行上的备注与标签。
        /// </summary>
        private void ApplyModeDefaults(QuoteFormMode mode)
        {
            if (mode != QuoteFormMode.New) return;

            RemarkBox.Text = string.Empty;
            FlagUrgent.IsChecked = false;
            FlagRec.IsChecked = false;
            FlagWhole.IsChecked = false;
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

        private static string FormatValuation(decimal? nzYtm)
        {
            return nzYtm.HasValue
                ? nzYtm.Value.ToString("0.0000", CultureInfo.InvariantCulture) + "%"
                : "—";
        }

        private static string FormatDate(DateTime value)
        {
            return value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private void SelectPriceType(PriceType priceType)
        {
            PriceTypeBox.SelectedIndex = priceType == PriceType.CleanPrice ? 2
                : priceType == PriceType.DirtyPrice ? 3
                : priceType == PriceType.YieldToExercise ? 1
                : 0;
        }

        /// <summary>清算速度下拉：请选择 / T+0 / T+1 / T+2 / T+3 / 远期（原型 6 项）。</summary>
        private void SelectSettlement(SettlementType settlement)
        {
            SettlementBox.SelectedIndex = settlement == SettlementType.T0 ? 1
                : settlement == SettlementType.T1 ? 2
                : settlement == SettlementType.T2 ? 3
                : settlement == SettlementType.T3 ? 4
                : settlement == SettlementType.Forward ? 5
                : 2;
        }

        #endregion

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

        private void OnTitleBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            try { DragMove(); }
            catch (Exception) { }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>原型 kbd 提示：Enter 保存修改、Esc 取消（备注多行输入不拦截 Enter）。</summary>
        private void OnWindowPreviewKeyDown(object sender, KeyEventArgs e)
        {
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
            Close();
        }

        /// <summary>校验后提交（原型 validateQuoteForm：价格/报量非空须为不小于 0 的有效数字）。
        /// 编辑（带报价 Id）走 update、新报价走 create，均经 <see cref="QuoteService"/>。</summary>
        private async void OnSubmit(object sender, RoutedEventArgs e)
        {
            ClearValidation();

            decimal price;
            if (!string.IsNullOrWhiteSpace(PriceBox.Text)
                && (!decimal.TryParse(PriceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out price) || price < 0m))
            {
                MarkInvalid(PriceBox, PriceErrorText, "请输入不小于0的有效价格");
                return;
            }

            decimal volume;
            if (!string.IsNullOrWhiteSpace(VolumeBox.Text)
                && (!decimal.TryParse(VolumeBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out volume) || volume < 0m))
            {
                MarkInvalid(VolumeBox, VolumeErrorText, "请输入不小于0的有效报量");
                return;
            }

            if (DirBid.IsChecked != true && DirOfr.IsChecked != true)
            {
                ToastService.Show("请选择报价方向（Bid / Ofr）", ToastKind.Warning);
                return;
            }

            if (QuoteService == null)
            {
                ToastService.Show("录入服务未就绪（数据源未装配）", ToastKind.Error);
                return;
            }

            // 新报价走 create+confirm 两步生效：confirm 只认 CRM Id，未选定直接拦截（方案 §3.1.4 专项 2）
            if (string.IsNullOrEmpty(editQuoteId) && string.IsNullOrEmpty(InstitutionBox.SelectedValue))
            {
                ToastService.Show("请先从联想列表选定报价机构（新报价需确认生效，仅名称无法确认）", ToastKind.Warning);
                return;
            }

            var row = BuildQuoteRow();
            try
            {
                SubmitButton.IsEnabled = false;
                if (!string.IsNullOrEmpty(editQuoteId))
                {
                    await QuoteService.UpdateQuotesAsync(new[] { row }, CancellationToken.None);
                    ToastService.Show("已保存修改（" + row.SecurityId + "）", ToastKind.Success);
                }
                else
                {
                    await QuoteService.SubmitQuotesAsync(new[] { row }, CancellationToken.None);
                    ToastService.Show("已提交新报价（" + row.SecurityId + "）", ToastKind.Success);
                }
                Close();
            }
            catch (Exception ex)
            {
                ToastService.Show("提交失败：" + ex.Message, ToastKind.Error);
            }
            finally
            {
                SubmitButton.IsEnabled = true;
            }
        }

        /// <summary>表单 → QuoteRow：编辑行带 QuoteId/Revision（乐观锁），新建行生成 clientQuoteId 供应答回绑。</summary>
        private QuoteRow BuildQuoteRow()
        {
            decimal price;
            decimal.TryParse(PriceBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out price);
            decimal volume;
            decimal.TryParse(VolumeBox.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out volume);

            var quoteType = PriceTypeBox.SelectedIndex == 2 ? PriceType.CleanPrice
                : PriceTypeBox.SelectedIndex == 3 ? PriceType.DirtyPrice
                : PriceTypeBox.SelectedIndex == 1 ? PriceType.YieldToExercise
                : PriceType.YieldToMaturity;
            var settlement = SettlementBox.SelectedIndex == 1 ? SettlementType.T0
                : SettlementBox.SelectedIndex == 3 ? SettlementType.T2
                : SettlementBox.SelectedIndex == 4 ? SettlementType.T3
                : SettlementBox.SelectedIndex == 5 ? SettlementType.Forward
                : SettlementType.T1;

            DateTime tradeDate;
            if (!DateTime.TryParse(TradeDateBox.Text, CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out tradeDate))
            {
                tradeDate = DateTime.Now.Date;
            }

            // 收益率计价才有 yieldType（到期/行权）；净价/全价必须为 null（后端约束）
            YieldType? yieldType;
            if (quoteType == PriceType.YieldToMaturity) yieldType = YieldType.ToMaturity;
            else if (quoteType == PriceType.YieldToExercise) yieldType = YieldType.ToExercise;
            else yieldType = null;

            return new QuoteRow
            {
                QuoteId = editQuoteId,
                Revision = editRevision,
                ClientQuoteId = string.IsNullOrEmpty(editQuoteId) ? "stc-" + Guid.NewGuid().ToString("N").Substring(0, 16) : null,
                SecurityId = BondCode.Text,
                SecurityName = BondName.Text,
                Side = DirOfr.IsChecked == true ? QuoteDirection.Ofr : QuoteDirection.Bid,
                QuoteType = quoteType,
                YieldType = yieldType,
                Price = price,
                Quantity = volume,
                TradeDate = tradeDate,
                SettlType = settlement,
                InstitutionName = InstitutionBox.Text,
                InstitutionId = InstitutionBox.SelectedValue,
                ContactName = TraderBox.Text,
                ContactId = TraderBox.SelectedValue,
                Urgent = FlagUrgent.IsChecked == true,
                Recommended = FlagRec.IsChecked == true,
                Whole = FlagWhole.IsChecked == true,
                Remark = RemarkBox.Text,
            };
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
