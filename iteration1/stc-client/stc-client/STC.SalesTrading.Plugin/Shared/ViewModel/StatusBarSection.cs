using System.ComponentModel;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>底部状态栏的单个分段（如：共 N 条 | Bid: n | Ofr: n | 同步状态）。
    /// Text 实现 INPC：VM 在查询后按 key 改文案时，状态栏才能跟着刷新
    /// （此前为普通属性，StatusSections[i].Text=... 的更新不会上屏）。</summary>
    public class StatusBarSection : INotifyPropertyChanged
    {
        private string _text;

        public StatusBarSection(string text)
        {
            _text = text;
        }

        /// <summary>分段文案。</summary>
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        /// <summary>可选强调色（Bid 橙 / Ofr 蓝 / 同步状态绿等），null 用默认色。</summary>
        public Brush Brush { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
