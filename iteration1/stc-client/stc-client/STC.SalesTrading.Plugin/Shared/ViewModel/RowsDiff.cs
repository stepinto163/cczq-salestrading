using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace STC.SalesTrading.Plugin.Shared.ViewModel
{
    /// <summary>
    /// 推送到达时的列表增量应用（替代 Clear+重建：保滚动位置、支撑新行闪烁）。
    /// 语义：移除消失的 key、按目标顺序 Move 对齐、原位替换内容（改价）、Insert 新增；
    /// 插入/替换且 key 命中 flashKeys（推送触及）的行回传给 View 播闪烁动画。
    /// 同一 UI 线程同步块内多次变更合并渲染，中间态不可见。
    /// </summary>
    internal static class RowsDiff
    {
        public static List<T> Apply<T>(ObservableCollection<T> target, IReadOnlyList<T> orderedRows,
            Func<T, string> key, ISet<string> flashKeys)
        {
            var flashed = new List<T>();

            // phase 1：移除消失的 key（撤单/挤出页），倒序避免索引位移
            var newKeys = new HashSet<string>(orderedRows.Select(key), StringComparer.Ordinal);
            for (int i = target.Count - 1; i >= 0; i--)
            {
                if (!newKeys.Contains(key(target[i]))) target.RemoveAt(i);
            }

            // phase 2：按目标顺序对齐。处理到 idx 时 target[0..idx-1] 已对齐，
            // 同 key 旧对象只会在 idx..end 之间
            for (int idx = 0; idx < orderedRows.Count; idx++)
            {
                var row = orderedRows[idx];
                var k = key(row);
                int cur = -1;
                for (int i = idx; i < target.Count; i++)
                {
                    if (string.Equals(key(target[i]), k, StringComparison.Ordinal)) { cur = i; break; }
                }

                if (cur >= 0)
                {
                    if (cur != idx) target.Move(cur, idx);      // updateTime 变化导致的排序移动
                    target[idx] = row;                          // 原位替换内容（改价）
                    if (flashKeys != null && flashKeys.Contains(k)) flashed.Add(row);
                }
                else
                {
                    target.Insert(idx, row);                    // 新报价插入
                    if (flashKeys != null && flashKeys.Contains(k)) flashed.Add(row);
                }
            }
            return flashed;
        }
    }
}
