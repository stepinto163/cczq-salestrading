using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using STC.Common.Utils.Events;
using STC.SalesTrading.Plugin.Abstractions;
using STC.SalesTrading.Plugin.Shared.Messages;
using STC.SalesTrading.Plugin.Shell;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace STC.SalesTrading.Plugin.Modules.QuoteEntry.ViewModel
{
    /// <summary>报价录入容器 VM：3 子页（报价确认/精确报价/模糊报价）切换。</summary>
    public partial class QuoteViewModel : ObservableObject, IDisposable
    {
        public QuoteViewModel(IQuoteService quoteService)
        {
            Confirm = new QuoteConfirmViewModel(quoteService);
            Precise = new PreciseQuoteViewModel(quoteService);
            Fuzzy = new FuzzyQuoteViewModel(quoteService);

            SubTabs = new ObservableCollection<SubTabItem>
            {
                new SubTabItem("报价确认", Confirm, "quote-confirm"),
                new SubTabItem("精确报价", Precise, "quote-precise"),
                // 模糊报价 tab 隐藏（方案 §3.1.3）：后端无此域（R9），模型与页面保留待后端上线
            };

            // 子页角标：报价确认 = 待确认报价数（原型 .subtab .cnt），与导航角标同源
            EventAggregator.Subscribe<PendingQuotesChangedMessage>(this, OnPendingQuotesChanged);

            // 首次加载可能与订阅同帧完成，这里补一次初值，避免角标缺失
            var confirmTab = SubTabs.FirstOrDefault(t => t.Key == "quote-confirm");
            if (confirmTab != null)
            {
                confirmTab.BadgeCount = Confirm.TotalRowCount;
            }

            _selectedSubTab = SubTabs[0];
        }

        public QuoteConfirmViewModel Confirm { get; }

        public PreciseQuoteViewModel Precise { get; }

        public FuzzyQuoteViewModel Fuzzy { get; }

        public ObservableCollection<SubTabItem> SubTabs { get; }

        [ObservableProperty]
        private SubTabItem _selectedSubTab;

        [RelayCommand]
        private void SwitchSubTab(SubTabItem item)
        {
            if (item != null) SelectedSubTab = item;
        }

        /// <summary>报价确认子页角标（按 Key 定位，避免依赖子页顺序）。</summary>
        private void OnPendingQuotesChanged(PendingQuotesChangedMessage message)
        {
            var tab = SubTabs.FirstOrDefault(t => t.Key == "quote-confirm");
            if (tab != null)
            {
                tab.BadgeCount = message.PendingCount;
            }
        }

        public void Dispose()
        {
            EventAggregator.Unsubscribe(this);
            Confirm.Dispose();
            Precise.Dispose();
            Fuzzy.Dispose();
        }
    }
}
