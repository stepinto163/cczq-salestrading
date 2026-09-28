using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 视图常驻的内容宿主：按 Content 实例缓存 ContentPresenter，切换时仅翻转 Visibility，不重建控件树。
    /// 用法：Content 绑定当前页/子页 VM，配套在 Resources 里放隐式 DataTemplate（按 VM 类型 → View），
    /// 替代「Grid 预置 N 个视图 + RelativeSource Visibility 长绑定」的手写常驻样板。
    /// 注意：缓存按 Content 实例（引用相等）键控，适用于子页数量有限、VM 实例固定的导航场景。
    /// </summary>
    public class KeepAliveContentControl : Grid
    {
        private readonly Dictionary<object, ContentPresenter> _cache = new Dictionary<object, ContentPresenter>();
        private ContentPresenter _current;

        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content", typeof(object), typeof(KeepAliveContentControl),
            new PropertyMetadata(null, (d, e) => ((KeepAliveContentControl)d).OnContentChanged(e.OldValue, e.NewValue)));

        /// <summary>当前展示的内容（通常是子页 VM）。同实例反复设置不产生任何操作。</summary>
        public object Content
        {
            get { return GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        private void OnContentChanged(object oldContent, object newContent)
        {
            if (ReferenceEquals(oldContent, newContent)) return;

            if (_current != null)
            {
                _current.Visibility = Visibility.Collapsed;
                _current = null;
            }

            if (newContent == null) return;

            ContentPresenter presenter;
            if (!_cache.TryGetValue(newContent, out presenter))
            {
                presenter = new ContentPresenter { Content = newContent };
                _cache[newContent] = presenter;
                Children.Add(presenter);
            }

            presenter.Visibility = Visibility.Visible;
            _current = presenter;
        }
    }
}
