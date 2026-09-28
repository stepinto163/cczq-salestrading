using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Modules.QuoteMgmt.View
{
    /// <summary>
    /// 报价管理视图：2 子页（今日报价 MgmtTodayView / 历史报价 MgmtHistoryView），
    /// 子页经 KeepAliveContentControl 常驻切换；选中行同步逻辑在子视图各自的 code-behind。
    /// </summary>
    public partial class QuoteMgmtView : UserControl
    {
        public QuoteMgmtView()
        {
            InitializeComponent();
        }
    }
}
