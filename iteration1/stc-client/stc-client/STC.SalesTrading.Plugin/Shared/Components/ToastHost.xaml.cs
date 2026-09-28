using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>全局 Toast 承载组件。挂在 Shell 根 Grid 上，经 ToastService.Show 弹消息。</summary>
    public partial class ToastHost : UserControl
    {
        public ToastHost()
        {
            InitializeComponent();
        }
    }
}
