using CommunityToolkit.Mvvm.ComponentModel;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.ViewModel
{
    /// <summary>
    /// 模糊报价行 VM（7 列：报价内容行内编辑/识别结果/机构/交易员/长城交易员/报价时间/状态 + 未同步红点）。
    /// </summary>
    public partial class IntentRowViewModel : ObservableObject
    {
        public IntentRowViewModel(Abstractions.Models.BoardIntentRow row)
        {
            Row = row;
            _content = row.Content ?? string.Empty;
        }

        /// <summary>原始 DTO（Refer/Unrefer 回写状态）。</summary>
        public Abstractions.Models.BoardIntentRow Row { get; }

        public string IntentId { get { return Row.IntentId; } }

        /// <summary>报价内容（可点击行内编辑）。</summary>
        [ObservableProperty]
        private string _content;

        public string RecognizedConditions { get { return Row.RecognizedConditions ?? string.Empty; } }
        public string Institution { get { return Row.Institution ?? string.Empty; } }
        public string Trader { get { return Row.Trader ?? string.Empty; } }
        public string WallTrader { get { return Row.WallTrader ?? string.Empty; } }
        public string QuoteTimeDisplay { get { return Row.QuoteTime?.ToString("HH:mm") ?? string.Empty; } }

        /// <summary>状态展示：有效/撤销。</summary>
        public string StatusDisplay
        {
            get
            {
                return Row.Status == Abstractions.Models.IntentStatus.Revoked ? "撤销" : "有效";
            }
        }

        /// <summary>是否已撤销（驱动状态文字颜色）。</summary>
        public bool IsRevoked
        {
            get { return Row.Status == Abstractions.Models.IntentStatus.Revoked; }
        }

        /// <summary>未同步（行前红色脉冲圆点）。</summary>
        public bool IsPendingSync
        {
            get { return Row.IsPendingSync; }
        }

        [ObservableProperty]
        private bool _isSelected;

        /// <summary>把行内编辑内容回写 DTO。</summary>
        public void Commit()
        {
            Row.Content = Content;
        }

        /// <summary>状态变更后由父 VM 调用，刷新状态相关绑定（父类不能跨实例调用受保护的 OnPropertyChanged）。</summary>
        public void NotifyStatusChanged()
        {
            OnPropertyChanged(nameof(StatusDisplay));
            OnPropertyChanged(nameof(IsRevoked));
        }
    }
}
