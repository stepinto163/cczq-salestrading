using STC.SalesTrading.Plugin.Themes;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shell.View
{
    /// <summary>
    /// 应用壳视图：顶部导航 + 页面切换 + 用户下拉 + Toast 浮层。
    /// 深色主题经 ThemeManager 合并到本控件资源，子树内全部 View/组件经 DynamicResource 取画刷。
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            ThemeManager.ApplyDark(this);
        }
    }
}
