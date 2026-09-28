using CommunityToolkit.Mvvm.ComponentModel;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>
    /// 模糊识别结果行（原型左栏 quote-fuzzy-table：☑ | 报价内容 | 识别结果 | 报价机构 | 报价交易员）。
    /// </summary>
    public partial class FuzzyParseRowViewModel : ObservableObject
    {
        /// <summary>客户端条目 Id（clientItemId）。</summary>
        public string ClientItemId { get; }

        public FuzzyParseRowViewModel(string clientItemId, string content)
        {
            ClientItemId = clientItemId;
            _content = content;
        }

        [ObservableProperty]
        private bool _isSelected;

        /// <summary>报价内容（等宽展示）。</summary>
        [ObservableProperty]
        private string _content;

        /// <summary>识别结果（结构化条件标签串，Mock 阶段为伪条件；真实条件集由识别服务返回）。</summary>
        [ObservableProperty]
        private string _conditionsLabel;

        /// <summary>报价机构（可编辑）。</summary>
        [ObservableProperty]
        private string _institution = string.Empty;

        /// <summary>报价交易员（可编辑）。</summary>
        [ObservableProperty]
        private string _trader = string.Empty;
    }
}
