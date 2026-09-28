using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using STC.SalesTrading.Plugin.Abstractions.Queries;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 查询加载/失败覆盖层（原六处逐字重复块的公共件）。
    /// State = Loading 显示加载遮罩；State = Error 显示失败信息 + 重试按钮。
    /// </summary>
    public partial class QueryStateOverlay : UserControl
    {
        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State), typeof(QueryLoadState), typeof(QueryStateOverlay), new PropertyMetadata(QueryLoadState.Loading));

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            nameof(ErrorMessage), typeof(string), typeof(QueryStateOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty RetryProperty = DependencyProperty.Register(
            nameof(Retry), typeof(ICommand), typeof(QueryStateOverlay), new PropertyMetadata(null));

        public QueryStateOverlay()
        {
            InitializeComponent();
        }

        /// <summary>查询状态（QueryLoadState.Loading / Error 时显示对应层）。</summary>
        public QueryLoadState State
        {
            get { return (QueryLoadState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        /// <summary>失败信息（Error 层展示）。</summary>
        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        /// <summary>重试命令（Error 层的「重试」按钮）。</summary>
        public ICommand Retry
        {
            get { return (ICommand)GetValue(RetryProperty); }
            set { SetValue(RetryProperty, value); }
        }
    }
}
