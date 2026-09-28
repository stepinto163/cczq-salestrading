using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 深色模态弹窗基类（遮罩 + 居中卡片 + 头部标题/✕ + 正文/页脚插槽）。
    /// 复刻原型的窗口壳：.modal-overlay(rgba(0,0,0,.6)) → .modal(880px 圆角卡片，border #30363d)
    /// + .modal-hdr（14px 20px 内距，底边框，标题 + 32×32 ✕）。
    /// 派生 XAML 用法：
    ///   &lt;v:ModalWindowBase ... ModalTitle="关注债券管理"&gt;
    ///       &lt;v:ModalWindowBase.ModalBody&gt; 正文（自带 Height） &lt;/v:ModalWindowBase.ModalBody&gt;
    ///       &lt;v:ModalWindowBase.ModalFooter&gt; 页脚按钮排（取消/保存） &lt;/v:ModalWindowBase.ModalFooter&gt;
    ///   &lt;/v:ModalWindowBase&gt;
    /// 注意：遮罩关闭处理必须挂在遮罩 Border 上而不能挂根容器——MouseDown 是冒泡事件，
    /// 挂根容器时点击弹窗内部任意非交互区域都会冒泡触发关闭（新建输入框失焦即关窗）；
    /// 卡片是遮罩的兄弟节点，内容事件不会冒泡进遮罩。
    /// </summary>
    public abstract class ModalWindowBase : Window
    {
        public static readonly DependencyProperty ModalTitleProperty = DependencyProperty.Register(
            nameof(ModalTitle), typeof(string), typeof(ModalWindowBase), new PropertyMetadata(null));

        public static readonly DependencyProperty ModalBodyProperty = DependencyProperty.Register(
            nameof(ModalBody), typeof(object), typeof(ModalWindowBase), new PropertyMetadata(null));

        public static readonly DependencyProperty ModalFooterProperty = DependencyProperty.Register(
            nameof(ModalFooter), typeof(object), typeof(ModalWindowBase), new PropertyMetadata(null));

        public static readonly DependencyProperty ModalWidthProperty = DependencyProperty.Register(
            nameof(ModalWidth), typeof(double), typeof(ModalWindowBase), new PropertyMetadata(880d, OnModalWidthChanged));

        private readonly TextBlock _titleText;
        private readonly ContentPresenter _bodyHost;
        private readonly ContentPresenter _footerHost;

        protected ModalWindowBase()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            ShowInTaskbar = false;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ResizeMode = ResizeMode.NoResize;
            FontFamily = new FontFamily("Segoe UI");
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;
            Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("/STC.SalesTrading.Plugin;component/Themes/DarkTheme.xaml", UriKind.Relative)
            });

            _titleText = new TextBlock { FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(20, 14, 20, 14) };
            _titleText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

            var closeButton = new Button
            {
                Content = "✕",
                Width = 32,
                Height = 32,
                FontSize = 13,
                Style = (Style)Resources["StcModalCloseButton"],
            };
            closeButton.Click += (s, e) => Close();

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            headerGrid.Children.Add(_titleText);
            Grid.SetColumn(closeButton, 1);
            headerGrid.Children.Add(closeButton);

            var headerBorder = new Border
            {
                Background = Brushes.Transparent,
                BorderBrush = (Brush)Resources["BorderStdBrush"],
                BorderThickness = new Thickness(0, 0, 0, 1),
                Child = headerGrid,
            };

            var bodyHost = new ContentPresenter();
            var footerHost = new ContentPresenter();
            _bodyHost = bodyHost;
            _footerHost = footerHost;

            var cardGrid = new Grid();
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            cardGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(headerBorder, 0);
            cardGrid.Children.Add(headerBorder);
            Grid.SetRow(bodyHost, 1);
            cardGrid.Children.Add(bodyHost);
            Grid.SetRow(footerHost, 2);
            cardGrid.Children.Add(footerHost);

            var card = new Border
            {
                Width = ModalWidth,
                Background = (Brush)Resources["PanelBgBrush"],
                BorderBrush = (Brush)Resources["BorderStdBrush"],
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Child = cardGrid,
            };

            // 遮罩：rgba(0,0,0,.6)，点空白关闭（处理挂遮罩自身，见类注释）
            var backdrop = new Border { Background = new SolidColorBrush(Color.FromArgb(0x99, 0, 0, 0)) };
            backdrop.MouseLeftButtonDown += (s, e) => Close();

            var root = new Grid();
            root.Children.Add(backdrop);
            root.Children.Add(card);
            Content = root;

            // 铺满宿主窗口压暗（对齐原型整窗遮罩；ConfirmDialog/PromptDialog 同款）
            Loaded += (s, e) => OverlayBackdrop.CoverOwner(this);
        }

        /// <summary>头部标题文案。</summary>
        public string ModalTitle
        {
            get { return (string)GetValue(ModalTitleProperty); }
            set { SetValue(ModalTitleProperty, value); }
        }

        /// <summary>正文内容（标题与页脚之间）。</summary>
        public object ModalBody
        {
            get { return GetValue(ModalBodyProperty); }
            set { SetValue(ModalBodyProperty, value); }
        }

        /// <summary>页脚内容（一般为 右对齐的 取消/保存 按钮排）。</summary>
        public object ModalFooter
        {
            get { return GetValue(ModalFooterProperty); }
            set { SetValue(ModalFooterProperty, value); }
        }

        /// <summary>卡片宽度（原型 .modal-wide = 880px）。</summary>
        public double ModalWidth
        {
            get { return (double)GetValue(ModalWidthProperty); }
            set { SetValue(ModalWidthProperty, value); }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ModalTitleProperty)
            {
                _titleText.Text = e.NewValue as string;
            }
            else if (e.Property == ModalBodyProperty)
            {
                _bodyHost.Content = e.NewValue;
            }
            else if (e.Property == ModalFooterProperty)
            {
                _footerHost.Content = e.NewValue;
            }
        }

        private static void OnModalWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 卡片宽度变化时同步（模态卡片宽度在 XAML 里静态设定，此处兜底）
            var win = (ModalWindowBase)d;
            var card = win.Content as Grid;
            if (card != null && card.Children.Count > 1)
            {
                var border = card.Children[1] as Border;
                if (border != null) border.Width = (double)e.NewValue;
            }
        }
    }
}
