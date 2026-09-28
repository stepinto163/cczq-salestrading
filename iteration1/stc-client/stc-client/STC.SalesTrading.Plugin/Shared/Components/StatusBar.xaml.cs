using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>底部状态栏组件。Sections 绑定 StatusBarSection 集合，可逐段着色（Bid 橙/Ofr 蓝/状态绿）。</summary>
    public partial class StatusBar : UserControl
    {
        public static readonly DependencyProperty SectionsProperty = DependencyProperty.Register(
            nameof(Sections), typeof(IEnumerable), typeof(StatusBar), new PropertyMetadata(null));

        public StatusBar()
        {
            InitializeComponent();
        }

        /// <summary>状态栏分段集合。</summary>
        public IEnumerable Sections
        {
            get { return (IEnumerable)GetValue(SectionsProperty); }
            set { SetValue(SectionsProperty, value); }
        }
    }
}
