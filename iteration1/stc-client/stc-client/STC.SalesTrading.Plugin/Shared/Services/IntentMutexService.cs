using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 模糊报价筛选面板的互斥提示（原型 INTENT_MUTEX_VALUES + checkIntentMutexWarning + renderIntentMutexBanner）。
    /// 语义：某条件组（如 城投属性）的**全部取值**都被选中时，等价于不筛选该条件 → 金色横幅提示 +
    /// 冲突 tag 加 .mutex-warn 虚线边框。tag 点击时调用 <see cref="Evaluate"/> 重算。
    /// 互斥组定义（原型 INTENT_MUTEX_VALUES）：
    ///   城投属性=[城投,非城投] / 是否免税=[免税,非免税] / 含权属性=[含权,不含权] /
    ///   永续债=[城投永续,产业永续,金融永续,非永续] / 次级债=[次级,非次级] / 上市状态=[上市,非上市]
    /// </summary>
    public static class IntentMutexService
    {
        private static readonly Dictionary<string, string[][]> MutexGroups = new Dictionary<string, string[][]>
        {
            { "城投属性", new[] { new[] { "城投", "非城投" } } },
            { "是否免税", new[] { new[] { "免税", "非免税" } } },
            { "含权属性", new[] { new[] { "含权", "不含权" } } },
            { "永续债", new[] { new[] { "城投永续", "产业永续", "金融永续", "非永续" } } },
            { "次级债", new[] { new[] { "次级", "非次级" } } },
            { "上市状态", new[] { new[] { "上市", "非上市" } } },
        };

        /// <summary>互斥提示文案列表（空 = 无冲突）。</summary>
        public static List<string> Warnings { get; private set; } = new List<string>();

        /// <summary>互斥提示变化时触发（IntentFilterPanel 重算横幅可见性与冲突 tag 样式）。</summary>
        public static event Action Changed;

        /// <summary>评估当前选中态并刷新冲突 tag 样式 + 横幅文案。</summary>
        /// <param name="panel">IntentFilterPanel 的根 ScrollViewer/StackPanel（用于枚举 ToggleButton）。</param>
        public static void Evaluate(Panel panel)
        {
            if (panel == null) return;

            // 收集每个组的选中值
            var selectedByGroup = new Dictionary<string, HashSet<string>>();
            foreach (var child in panel.Children.OfType<Panel>())
            {
                CollectSelected(child, selectedByGroup);
            }
            // 顶层直接挂的 tag 也要扫
            CollectSelected(panel, selectedByGroup);

            Warnings.Clear();
            foreach (var kv in MutexGroups)
            {
                if (!selectedByGroup.TryGetValue(kv.Key, out var selected)) continue;
                foreach (var group in kv.Value)
                {
                    if (group.All(v => selected.Contains(v)))
                    {
                        Warnings.Add("“" + string.Join(" + ", group) + "” 已覆盖全部取值，等价于不筛选该条件");
                    }
                }
            }

            // 冲突 tag 高亮：先清，再标
            foreach (var tb in FindAllTags(panel))
            {
                tb.ClearValue(ToggleButton.BorderBrushProperty);
                tb.ClearValue(ToggleButton.BorderThicknessProperty);
            }
            foreach (var kv in MutexGroups)
            {
                if (!selectedByGroup.TryGetValue(kv.Key, out var selected)) continue;
                foreach (var group in kv.Value)
                {
                    if (group.All(v => selected.Contains(v)))
                    {
                        foreach (var v in group)
                        {
                            var tb = FindTag(panel, kv.Key, v);
                            if (tb != null)
                            {
                                tb.BorderBrush = new SolidColorBrush(Color.FromRgb(0xD2, 0x99, 0x22));
                                tb.BorderThickness = new Thickness(1);
                            }
                        }
                    }
                }
            }

            Changed?.Invoke();
        }

        /// <summary>横幅文案（多条用换行符连接）。</summary>
        public static string BannerText
        {
            get { return Warnings.Count == 0 ? null : string.Join("\n", Warnings); }
        }

        private static void CollectSelected(Panel panel, Dictionary<string, HashSet<string>> selectedByGroup)
        {
            foreach (var child in panel.Children.OfType<FrameworkElement>())
            {
                if (child is ToggleButton tb && tb.IsChecked == true)
                {
                    var group = tb.Tag as string;
                    if (string.IsNullOrEmpty(group) || !MutexGroups.ContainsKey(group)) continue;
                    if (!selectedByGroup.TryGetValue(group, out var set))
                    {
                        set = new HashSet<string>();
                        selectedByGroup[group] = set;
                    }
                    set.Add(tb.Content as string ?? string.Empty);
                }
                else if (child is Panel sub)
                {
                    CollectSelected(sub, selectedByGroup);
                }
            }
        }

        private static ToggleButton FindTag(Panel panel, string group, string value)
        {
            foreach (var child in panel.Children.OfType<FrameworkElement>())
            {
                if (child is ToggleButton tb && (tb.Tag as string) == group && (tb.Content as string) == value)
                {
                    return tb;
                }
                if (child is Panel sub)
                {
                    var found = FindTag(sub, group, value);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private static IEnumerable<ToggleButton> FindAllTags(Panel panel)
        {
            foreach (var child in panel.Children.OfType<FrameworkElement>())
            {
                if (child is ToggleButton tb && !string.IsNullOrEmpty(tb.Tag as string))
                {
                    yield return tb;
                }
                if (child is Panel sub)
                {
                    foreach (var t in FindAllTags(sub)) yield return t;
                }
            }
        }
    }
}
