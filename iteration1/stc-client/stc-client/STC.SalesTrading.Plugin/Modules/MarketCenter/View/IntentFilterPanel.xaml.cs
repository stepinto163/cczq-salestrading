using System.Windows;
using System.Windows.Controls;
using STC.SalesTrading.Plugin.Shared.Services;

namespace STC.SalesTrading.Plugin.Modules.MarketCenter.View
{
    /// <summary>模糊报价筛选面板（原型 filters-intent，0910 改名）。</summary>
    public partial class IntentFilterPanel : UserControl
    {
        public IntentFilterPanel()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 首次评估（无选中时横幅隐藏）
            EvaluateMutex();
        }

        /// <summary>互斥 tag 点击 → 重新评估（原型 checkIntentMutexWarning + renderIntentMutexBanner）。</summary>
        private void OnMutexTagClick(object sender, RoutedEventArgs e)
        {
            EvaluateMutex();
        }

        /// <summary>横幅 ✕ 关闭（原型 onclick="this.parentElement.remove()"，WPF 退化为隐藏）。</summary>
        private void OnMutexBannerClose(object sender, RoutedEventArgs e)
        {
            MutexBanner.Visibility = Visibility.Collapsed;
        }

        private void EvaluateMutex()
        {
            IntentMutexService.Evaluate(RootPanel);
            var text = IntentMutexService.BannerText;
            if (string.IsNullOrEmpty(text))
            {
                MutexBanner.Visibility = Visibility.Collapsed;
            }
            else
            {
                MutexBannerText.Text = text;
                MutexBanner.Visibility = Visibility.Visible;
            }
        }
    }
}
