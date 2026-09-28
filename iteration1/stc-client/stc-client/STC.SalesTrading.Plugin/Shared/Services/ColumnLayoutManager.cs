using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace STC.SalesTrading.Plugin.Shared.Services
{
    /// <summary>
    /// 表格列布局持久化（原型 localStorage 列拖拽/列宽记忆语义）。
    /// 落盘到 %LOCALAPPDATA%\STC\SalesTrading\column-layouts.txt（避免写仓库/宿主目录）。
    /// 格式：每行 "表标识|列名=索引,宽度;列名=索引,宽度;..."，仅保存与当前列集可对应的列。
    /// </summary>
    public static class ColumnLayoutManager
    {
        private static readonly object Gate = new object();
        private static Dictionary<string, Dictionary<string, string>> _cache;

        private static string LayoutFilePath
        {
            get
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "STC", "SalesTrading");
                return Path.Combine(dir, "column-layouts.txt");
            }
        }

        private static Dictionary<string, Dictionary<string, string>> LoadAll()
        {
            if (_cache != null) return _cache;
            var map = new Dictionary<string, Dictionary<string, string>>();
            try
            {
                var file = LayoutFilePath;
                if (File.Exists(file))
                {
                    foreach (var line in File.ReadAllLines(file))
                    {
                        var sep = line.IndexOf('|');
                        if (sep <= 0) continue;
                        var key = line.Substring(0, sep);
                        var cols = new Dictionary<string, string>();
                        foreach (var part in line.Substring(sep + 1).Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var eq = part.IndexOf('=');
                            if (eq <= 0) continue;
                            cols[part.Substring(0, eq)] = part.Substring(eq + 1);
                        }
                        map[key] = cols;
                    }
                }
            }
            catch (Exception)
            {
                // 读取失败按无布局处理
            }
            _cache = map;
            return map;
        }

        /// <summary>应用已保存的列序与宽度（按列头匹配，忽略已不存在的列）。</summary>
        public static void Apply(DataGrid grid, string key)
        {
            try
            {
                var cols = LoadAll();
                Dictionary<string, string> saved;
                if (!cols.TryGetValue(key, out saved)) return;

                var byHeader = grid.Columns.ToDictionary(
                    c => (c.Header?.ToString() ?? string.Empty),
                    c => c,
                    StringComparer.Ordinal);

                foreach (var kv in saved)
                {
                    DataGridColumn column;
                    if (!byHeader.TryGetValue(kv.Key, out column)) continue;

                    var parts = kv.Value.Split(',');
                    int index;
                    double width;
                    if (parts.Length > 0 && int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out index))
                    {
                        var max = grid.Columns.Count - 1;
                        if (index >= 0 && index <= max && column.DisplayIndex != index)
                        {
                            try { column.DisplayIndex = index; } catch (Exception) { }
                        }
                    }
                    if (parts.Length > 1 && double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out width))
                    {
                        if (width > 20 && Math.Abs(column.Width.Value - width) > 1)
                        {
                            column.Width = new DataGridLength(width);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 布局还原失败不影响功能
            }
        }

        /// <summary>保存当前列序与宽度（防抖由调用方控制）。</summary>
        public static void Save(DataGrid grid, string key)
        {
            lock (Gate)
            {
                try
                {
                    var cols = LoadAll();
                    var entries = grid.Columns
                        .Select(c => string.Format(CultureInfo.InvariantCulture, "{0}={1},{2}",
                            c.Header?.ToString() ?? string.Empty,
                            c.DisplayIndex,
                            c.Width.IsAbsolute ? c.Width.Value : c.ActualWidth))
                        .Where(e => e.StartsWith("=") == false);

                    cols[key] = entries.ToDictionary(
                        e => e.Substring(0, e.IndexOf('=')),
                        e => e.Substring(e.IndexOf('=') + 1),
                        StringComparer.Ordinal);

                    var file = LayoutFilePath;
                    var dir = Path.GetDirectoryName(file);
                    if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllLines(file, cols.Select(kv => kv.Key + "|" + string.Join(";", kv.Value.Select(c => c.Key + "=" + c.Value))));
                }
                catch (Exception)
                {
                    // 落盘失败静默（无权限/只读环境）
                }
            }
        }
    }
}
