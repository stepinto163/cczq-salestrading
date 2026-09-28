using CommunityToolkit.Mvvm.ComponentModel;

namespace STC.SalesTrading.Plugin.Shell
{
    /// <summary>
    /// 模块内子页 tab 项。Title 仅为展示文案,切换逻辑以 Target(子页 VM 或视图模式枚举)为准。
    /// </summary>
    public class SubTabItem : ObservableObject
    {
        public SubTabItem(string title, object target, string key = "")
        {
            Title = title;
            Target = target;
            Key = key;
        }

        /// <summary>展示文案。</summary>
        public string Title { get; }

        /// <summary>选中后切换到的目标。</summary>
        public object Target { get; }

        /// <summary>稳定逻辑标识（视图常驻 Visibility 切换用）。</summary>
        public string Key { get; }

        private int _badgeCount;

        /// <summary>
        /// 子页角标数（原型 .subtab .cnt；0 隐藏）。
        /// 报价确认 = 待确认报价数，报价提醒 = 未读提醒数，来源同导航角标消息。
        /// </summary>
        public int BadgeCount
        {
            get { return _badgeCount; }
            set { SetProperty(ref _badgeCount, value); }
        }
    }
}
