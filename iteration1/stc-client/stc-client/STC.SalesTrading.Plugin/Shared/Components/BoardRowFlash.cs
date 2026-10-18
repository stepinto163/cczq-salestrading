using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace STC.SalesTrading.Plugin.Shared.Components
{
    /// <summary>
    /// 看板行推送高亮（新报价/改价琥珀渐隐）。表格开了行容器 Recycling（StcDataGrid），
    /// 容器会被复用到其它数据行：播放前先清掉容器上可能残留的旧动画；
    /// 动画中途容器被回收换数据时，经 DataContextChanged 立即终止，避免新行闪旧色。
    /// </summary>
    internal static class BoardRowFlash
    {
        public static void Flash(System.Windows.Controls.DataGrid grid, IEnumerable<object> items)
        {
            // 集合变更（Move/原位替换）的同一消息内容器尚未生成（ContainerFromIndex 返回 null，
            // 实测探针 index=0 row=False）——推迟到 Loaded 优先级（布局完成、容器就绪）后执行
            var snapshot = items.ToList();
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher == null)
            {
                FlashCore(grid, snapshot);
                return;
            }
            dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded,
                new Action(() => FlashCore(grid, snapshot)));
        }

        private static void FlashCore(System.Windows.Controls.DataGrid grid, System.Collections.Generic.List<object> items)
        {
            foreach (var item in items)
            {
                // 按索引解析容器（增量应用后实例已换新，Index 不依赖 item→container 映射）
                var index = grid.Items.IndexOf(item);
                var row = index >= 0 ? grid.ItemContainerGenerator.ContainerFromIndex(index) as DataGridRow : null;
                if (row == null) continue;                                      // 行不在视口（虚拟化），跳过

                StopFlash(row);                                                 // 同容器二次闪烁/残留动画清理

                var flash = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B45309"))
                {
                    Opacity = 0.55,
                };
                row.Background = flash;                                         // 本地值，结束后清掉还原样式触发器
                var anim = new ColorAnimation
                {
                    From = (Color)ColorConverter.ConvertFromString("#B45309"),
                    To = Colors.Transparent,
                    Duration = new Duration(TimeSpan.FromSeconds(1.6)),
                };
                anim.Completed += (s, e) => StopFlash(row);
                row.DataContextChanged += OnFlashRowDataContextChanged;
                flash.BeginAnimation(SolidColorBrush.ColorProperty, anim);
            }
        }

        private static void OnFlashRowDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            StopFlash((DataGridRow)sender);                                     // 容器复用给别的行：终止残留闪烁
        }

        /// <summary>终止行闪烁：停掉画刷动画、清 Background 本地值、解挂 DataContext 监听。
        /// 主题字典里的样式画刷是冻结的（IsFrozen），对它调 BeginAnimation 抛"已密封或已冻结"
        /// 并顺着 RowsFlashed 冒进 QueryFlow 变成查询失败——只动我们自己挂的本地值。</summary>
        private static void StopFlash(DataGridRow row)
        {
            row.DataContextChanged -= OnFlashRowDataContextChanged;
            var brush = row.Background as SolidColorBrush;
            if (brush != null && !brush.IsFrozen)
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
            }
            if (row.ReadLocalValue(DataGridRow.BackgroundProperty) != System.Windows.DependencyProperty.UnsetValue)
            {
                row.ClearValue(DataGridRow.BackgroundProperty);
            }
        }
    }
}
