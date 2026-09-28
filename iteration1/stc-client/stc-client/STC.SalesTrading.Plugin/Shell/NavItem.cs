using CommunityToolkit.Mvvm.ComponentModel;

namespace STC.SalesTrading.Plugin.Shell
{
    /// <summary>
    /// 顶部导航项。Title 仅为展示文案,切换逻辑以 Target(目标 VM/视图模式)为准,
    /// 禁止用中文文案作逻辑标识符(改文案即断,且无编译错误)。
    /// </summary>
    public class NavItem : ObservableObject
    {
        public NavItem(string title, object target, string key = "")
        {
            Title = title;
            Target = target;
            Key = key;
        }

        /// <summary>展示文案。</summary>
        public string Title { get; }

        /// <summary>选中后切换到的目标(页面 VM 实例)。</summary>
        public object Target { get; }

        /// <summary>稳定逻辑标识（视图常驻 Visibility 切换用；Title 仅展示，禁止当 key）。</summary>
        public string Key { get; }

        private int _badgeCount;

        /// <summary>右上角角标数(0 隐藏)。</summary>
        public int BadgeCount
        {
            get { return _badgeCount; }
            set { SetProperty(ref _badgeCount, value); }
        }
    }
}
