using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 「有 N 条新报价，点击刷新」横幅（原信用全部/信用最优两视图逐字重复块的公共件）。
    /// 绑定宿主 DataContext 的 RefreshPendingQuotesCommand / PendingNewQuoteCount，自身无逻辑。
    /// </summary>
    public partial class PendingQuotesBanner : UserControl
    {
        public PendingQuotesBanner()
        {
            InitializeComponent();
        }
    }
}
