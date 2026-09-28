using Newtonsoft.Json;
using STC.Common.Model;
using STC.Common.ViewModel.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
namespace STC.PP.Plugin.Handler
{
    public class DataGridConfigManager
    {
        private static DataGridConfigManager server = new DataGridConfigManager();
        public static DataGridConfigManager Instance { get { return server; } }

        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"data", "DgPPConfig.json");
        public DataGridConfigManager()
        {

        }

        /// <summary>
        /// 保存 DataGrid 配置
        /// </summary>
        public void SaveConfig(DataGrid grid, string gridName)
        {
            if (string.IsNullOrEmpty(gridName)) return;

            MsgAlertDataModel allConfigs = LoadAllConfigs();
            var config = new MsgAlertModel();
            config.id = "1";
            config.code = "ShowPositionField";
            config.name = "";
            int i = 0;
            foreach (var col in grid.Columns)
            {
                string headerText = GetColumnHeaderText(col);
                if (string.IsNullOrEmpty(headerText))
                    continue;

                config.subList.Add(new MsgAlertModel
                {
                    name = headerText,
                    Width = col.Width.DisplayValue,
                    ischecked = col.Visibility == Visibility.Visible,
                    code = col.DisplayIndex.ToString()
                });
            }

            SaveAllConfigs(allConfigs);
        }

        public MsgAlertDataModel GetDefaultColumn(DataGrid grid)
        {
            MsgAlertDataModel config = new MsgAlertDataModel();
            config.id = "1";
            config.code = "ShowPositionField";
            config.name = "";
            config.subList = new List<MsgAlertModel>();
            int i = 0;
            foreach (var col in grid.Columns)
            {
                string headerText = GetColumnHeaderText(col);
                if (string.IsNullOrEmpty(headerText))
                    continue;

                config.subList.Add(new MsgAlertModel
                {
                    name = headerText,
                    Width = col.Width.DisplayValue,
                    ischecked = col.Visibility == Visibility.Visible,
                    isenabled = headerText == "组合名称" || headerText == "债券代码" || headerText == "债券名称" ? false : true,
                    code = col.DisplayIndex.ToString()
                });
            }

            return config;
        }

        /// <summary>
        /// 应用配置到 DataGrid
        /// </summary>
        public  void ApplyConfig(DataGrid grid, string gridName)
        {
            if (string.IsNullOrEmpty(gridName)) return;

            MsgAlertDataModel savedConfig = LoadAllConfigs();
            if (savedConfig?.subList == null)
                return;

            // 先按配置顺序重新排列列
            foreach (var cfg in savedConfig.subList.OrderBy(c => c.code))
            {
                var col = grid.Columns.FirstOrDefault(c =>
                    string.Equals(GetColumnHeaderText(c), cfg.name, StringComparison.Ordinal));

                if (col != null)
                {
                    col.Width = new DataGridLength(cfg.Width);
                    col.Visibility = cfg.ischecked ? Visibility.Visible : Visibility.Collapsed;
                    col.DisplayIndex = int.Parse(cfg.code ?? "0");
                }
            }

            //// 配置中不存在的列隐藏
            //foreach (var col in grid.Columns)
            //{
            //    string headerText = GetColumnHeaderText(col);
            //    if (!savedConfig.Columns.Any(c => c.Header == headerText))
            //        col.Visibility = Visibility.Collapsed;
            //}
        }

        /// <summary>
        /// 获取列头文本（支持 HeaderTemplate、StackPanel、CheckBox.Content 等多层嵌套）
        /// </summary>
        private string GetColumnHeaderText(DataGridColumn col)
        {
            // 1️⃣ Header 为字符串
            var headerStr = col.Header as string;
            if (!string.IsNullOrEmpty(headerStr) && headerStr.Trim().Length > 0)
                return headerStr.Trim();

            // 2️⃣ Header 为 TextBlock
            var headerTextBlock = col.Header as TextBlock;
            if (headerTextBlock != null &&
                !string.IsNullOrEmpty(headerTextBlock.Text) && headerTextBlock.Text.Trim().Length > 0)
                return headerTextBlock.Text.Trim();

            // 3️⃣ Header 为 CheckBox
            var headerCheckBox = col.Header as CheckBox;
            if (headerCheckBox != null)
            {
                var cbContentStr = headerCheckBox.Content as string;
                if (!string.IsNullOrEmpty(cbContentStr) && cbContentStr.Trim().Length > 0)
                    return cbContentStr.Trim();

                var cbContentTextBlock = headerCheckBox.Content as TextBlock;
                if (cbContentTextBlock != null &&
                    !string.IsNullOrEmpty(cbContentTextBlock.Text) && cbContentTextBlock.Text.Trim().Length > 0)
                    return cbContentTextBlock.Text.Trim();
            }

            // 4️⃣ HeaderTemplate 的情况
            if (col.HeaderTemplate != null)
            {
                var content = col.HeaderTemplate.LoadContent();

                // 单独是 TextBlock
                var tb = content as TextBlock;
                if (tb != null && !string.IsNullOrEmpty(tb.Text) && tb.Text.Trim().Length > 0)
                    return tb.Text.Trim();

                // 单独是 CheckBox
                var cb = content as CheckBox;
                if (cb != null && cb.Content != null)
                {
                    var cc = cb.Content as string;
                    if (!string.IsNullOrEmpty(cc) && cc.Trim().Length > 0)
                        return cc.Trim();

                    var cct = cb.Content as TextBlock;
                    if (cct != null && !string.IsNullOrEmpty(cct.Text) && cct.Text.Trim().Length > 0)
                        return cct.Text.Trim();
                }

                // 嵌套结构
                var fe = content as FrameworkElement;
                if (fe != null)
                {
                    var text = FindTextBlockOrCheckBoxText(fe);
                    if (!string.IsNullOrEmpty(text) && text.Trim().Length > 0)
                        return text.Trim();
                }
            }

            return null;
        }


        /// <summary>
        /// 递归查找 FrameworkElement 内第一个 TextBlock.Text 或 CheckBox.Content
        /// </summary>
        private  string FindTextBlockOrCheckBoxText(DependencyObject parent)
        {
            if (parent == null)
                return null;

            int childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);

                // TextBlock
                var tb = child as TextBlock;
                if (tb != null && tb.Text != null && tb.Text.Trim().Length > 0)
                    return tb.Text.Trim();

                // CheckBox
                var cb = child as CheckBox;
                if (cb != null)
                {
                    var cs = cb.Content as string;
                    if (!string.IsNullOrEmpty(cs) && cs.Trim().Length > 0)
                        return cs.Trim();

                    var cbt = cb.Content as TextBlock;
                    if (cbt != null && !string.IsNullOrEmpty(cbt.Text) && cbt.Text.Trim().Length > 0)
                        return cbt.Text.Trim();
                }

                // 递归查找子元素
                var text = FindTextBlockOrCheckBoxText(child);
                if (!string.IsNullOrEmpty(text) && text.Trim().Length > 0)
                    return text.Trim();
            }

            return null;
        }


        public MsgAlertDataModel LoadAllConfigs()
        {
            if (!File.Exists(ConfigPath))
                return new MsgAlertDataModel();

            try
            {
                string json = File.ReadAllText(ConfigPath);
                return Common.Utils.CommonUtils.DeserializeObject<MsgAlertDataModel>(json)?? new MsgAlertDataModel();
            }
            catch
            {
                return new MsgAlertDataModel();
            }
        }

        public void SaveAllConfigs(MsgAlertDataModel configs)
        {
           string json = JsonConvert.SerializeObject(configs, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            File.WriteAllText(ConfigPath, json);
        }

    }
}
