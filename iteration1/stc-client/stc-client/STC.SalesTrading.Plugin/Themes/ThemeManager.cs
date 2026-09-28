using System;
using System.Windows;

namespace STC.SalesTrading.Plugin.Themes
{
    /// <summary>
    /// 主题管理。当前仅深色主题（复刻原型 GitHub-Dark 配色，见 Themes/Colors.xaml 的逐条摘录）。
    /// 多皮肤时替换 MergedDictionaries 中的主题字典即可；样式/画刷引用统一走 DynamicResource，
    /// 换肤后自动生效。注意：销交平台皮肤独立于宿主全局换肤，样式全部 keyed、无隐式样式，
    /// 避免污染宿主与兄弟插件的弹窗。
    /// </summary>
    public static class ThemeManager
    {
        private const string DarkThemeUri = "/STC.SalesTrading.Plugin;component/Themes/DarkTheme.xaml";

        /// <summary>给指定根元素合并深色主题（幂等）。</summary>
        public static void ApplyDark(FrameworkElement root)
        {
            if (root == null) return;

            var uri = new Uri(DarkThemeUri, UriKind.RelativeOrAbsolute);
            foreach (var dictionary in root.Resources.MergedDictionaries)
            {
                if (dictionary.Source == uri) return;
            }

            root.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }
    }
}
