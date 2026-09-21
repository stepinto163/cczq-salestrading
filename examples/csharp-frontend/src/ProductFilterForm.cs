using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.FrontendLearning {
    // Numeric/text fields are edited directly; categorical and boolean fields have a picker.
    internal sealed class ProductFilterForm : Form
    {
        private readonly DataGridView grid = new DataGridView();
        internal JObject Filter { get; private set; }

        internal ProductFilterForm(JArray fields, JObject filter, JObject dictionaries = null)
        {
            Text = "完整条件筛选：报价 / 债券 / 流动性 / 信用 / 条款 / 区域";
            Width = 1000;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "field", HeaderText = "条件", ReadOnly = true });
            grid.Columns.Add(new DataGridViewComboBoxColumn { Name = "operator", HeaderText = "运算符" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "value", HeaderText = "值（留空不限；范围/多选用逗号分隔）" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "dictionary", HeaderText = "选择 / 输入方式", ReadOnly = true });

            foreach (JObject source in fields)
            {
                var field = (JObject)source.DeepClone();
                if (!(field["enumValues"] is JArray) && dictionaries?[(string)field["field"]] is JArray options)
                {
                    field["enumValues"] = options.DeepClone();
                }
                int index = grid.Rows.Add();
                var row = grid.Rows[index];
                row.Tag = field;
                row.Cells[0].Value = (string)field["group"] + " / " + ((string)field["label"] ?? (string)field["field"]);
                var operators = (DataGridViewComboBoxCell)row.Cells[1];
                operators.Items.AddRange(((JArray)field["operators"]).Values<string>().Cast<object>().ToArray());
                var condition = ((JArray)filter["conditions"] ?? new JArray()).OfType<JObject>()
                    .FirstOrDefault(item => (string)item["field"] == (string)field["field"]);
                row.Cells[1].Value = (string)condition?["operator"] ?? "EQ";
                row.Cells[2].Value = condition?["value"] is JArray values
                    ? string.Join(",", values.Select(value => value.ToString()))
                    : condition?["value"]?.ToString();
                row.Cells[2].ToolTipText = field["type"] + "; " + field["field"]
                    + ((bool?)field["includesSimulatedData"] == true ? "（含模拟资料）" : "");
                RefreshPickerCell(row);
            }

            grid.CellContentClick += (sender, args) =>
            {
                if (args.RowIndex >= 0 && args.ColumnIndex == 3
                    && grid.Rows[args.RowIndex].Cells[3] is DataGridViewButtonCell)
                {
                    ChooseDictionaryValues(grid.Rows[args.RowIndex]);
                }
            };
            grid.CurrentCellDirtyStateChanged += (sender, args) =>
            {
                if (grid.IsCurrentCellDirty && grid.CurrentCell?.ColumnIndex == 1)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.CellValueChanged += (sender, args) =>
            {
                if (args.RowIndex >= 0 && args.ColumnIndex == 1)
                {
                    RefreshPickerCell(grid.Rows[args.RowIndex]);
                }
            };

            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60 };
            var apply = new Button { Text = "应用", AutoSize = true };
            apply.Click += (sender, args) => Apply();
            var clear = new Button { Text = "清除全部", AutoSize = true };
            clear.Click += (sender, args) =>
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    row.Cells[2].Value = "";
                }
            };
            bar.Controls.Add(apply);
            bar.Controls.Add(clear);
            bar.Controls.Add(new Label
            {
                Text = "分类/是否条件点“选择”；数值和文字直接填写“值”。字典未提供时请更新服务端。",
                AutoSize = true
            });
            Controls.Add(grid);
            Controls.Add(bar);
        }

        private static JArray PickerOptions(JObject field, string operation)
        {
            if (operation == "EXISTS" || (string)field["type"] == "boolean")
            {
                return new JArray(
                    new JObject { ["value"] = true, ["label"] = operation == "EXISTS" ? "有值" : "是" },
                    new JObject { ["value"] = false, ["label"] = operation == "EXISTS" ? "无值" : "否" });
            }
            return field["enumValues"] as JArray;
        }

        private void RefreshPickerCell(DataGridViewRow row)
        {
            var field = row.Tag as JObject;
            if (field == null)
            {
                return;
            }
            var options = PickerOptions(field, Convert.ToString(row.Cells[1].Value));
            if (options != null && options.Count > 0)
            {
                row.Cells[3] = new DataGridViewButtonCell { Value = "选择" };
                return;
            }
            string name = (string)field["field"];
            bool directInput = (string)field["type"] == "decimal"
                || new[] { "searchText", "securityId", "securityName", "issuerId", "issuerName", "guarantorName" }.Contains(name);
            row.Cells[3] = new DataGridViewTextBoxCell
            {
                Value = directInput ? "直接输入" : "字典未提供",
                ToolTipText = directInput ? "请在左侧“值”单元格输入；区间用 BETWEEN。" : "服务端未返回此字段的选项字典，请更新服务端后重新打开筛选。",
                Style = new DataGridViewCellStyle { ForeColor = System.Drawing.SystemColors.GrayText }
            };
        }

        private void ChooseDictionaryValues(DataGridViewRow row)
        {
            grid.EndEdit();
            var field = (JObject)row.Tag;
            string operation = Convert.ToString(row.Cells[1].Value);
            var options = PickerOptions(field, operation);
            if (options == null || options.Count == 0)
            {
                RefreshPickerCell(row);
                return;
            }
            bool singleChoice = operation == "EXISTS" || (string)field["type"] == "boolean";
            using (var picker = new Form())
            using (var choices = new CheckedListBox())
            {
                picker.Text = (string)field["label"];
                picker.Width = 360;
                picker.Height = 420;
                picker.StartPosition = FormStartPosition.CenterParent;
                choices.Dock = DockStyle.Fill;
                choices.CheckOnClick = true;
                string[] selected = Convert.ToString(row.Cells[2].Value)
                    .Split(new[] { ',', '，' }).Select(value => value.Trim()).ToArray();
                foreach (JObject option in options)
                {
                    int index = choices.Items.Add((string)option["label"] + "  [" + option["value"] + "]");
                    choices.SetItemChecked(index, selected.Contains(option["value"].ToString())
                        || selected.Contains((string)option["label"]));
                }
                if (singleChoice)
                {
                    choices.ItemCheck += (sender, args) =>
                    {
                        if (args.NewValue == CheckState.Checked)
                        {
                            foreach (int index in choices.CheckedIndices.Cast<int>().ToArray())
                            {
                                if (index != args.Index)
                                {
                                    choices.SetItemChecked(index, false);
                                }
                            }
                        }
                    };
                }
                var confirm = new Button { Text = "确定", Dock = DockStyle.Bottom, Height = 32 };
                confirm.Click += (sender, args) => picker.DialogResult = DialogResult.OK;
                picker.Controls.Add(choices);
                picker.Controls.Add(confirm);
                if (picker.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                var codes = choices.CheckedIndices.Cast<int>()
                    .Select(index => options[index]["value"].ToString()).ToArray();
                row.Cells[1].Value = operation == "EXISTS" ? "EXISTS" : codes.Length > 1 ? "IN" : "EQ";
                row.Cells[2].Value = string.Join(",", codes);
            }
        }

        private void Apply()
        {
            try
            {
                grid.EndEdit();
                var conditions = new JArray();
                foreach (DataGridViewRow row in grid.Rows)
                {
                    string text = Convert.ToString(row.Cells[2].Value).Trim();
                    if (text.Length == 0)
                    {
                        continue;
                    }
                    var field = (JObject)row.Tag;
                    string operation = (string)row.Cells[1].Value;
                    var parts = text.Split(new[] { ',', '，' }).Select(value => value.Trim()).ToArray();
                    Func<string, JToken> parse = value =>
                    {
                        if (operation == "EXISTS" || (string)field["type"] == "boolean")
                        {
                            return new JValue(bool.Parse(value));
                        }
                        if ((string)field["type"] == "decimal")
                        {
                            decimal.Parse(value, CultureInfo.InvariantCulture);
                        }
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            throw new FormatException("条件值不能为空");
                        }
                        if (field["enumValues"] is JArray options)
                        {
                            var option = options.OfType<JObject>().FirstOrDefault(item =>
                                (string)item["value"] == value || (string)item["label"] == value);
                            if (option == null)
                            {
                                throw new FormatException("请选择字典中的名称或代码：" + (string)field["label"]);
                            }
                            return option["value"].DeepClone();
                        }
                        return new JValue(value);
                    };
                    if (operation == "BETWEEN" && (parts.Length != 2
                        || decimal.Parse(parts[0], CultureInfo.InvariantCulture) > decimal.Parse(parts[1], CultureInfo.InvariantCulture)))
                    {
                        throw new FormatException("范围请输入下限,上限，并且下限不能大于上限");
                    }
                    JToken valueToken = operation == "IN" || operation == "BETWEEN"
                        ? new JArray(parts.Select(parse)) : parse(text);
                    conditions.Add(new JObject
                    {
                        ["field"] = field["field"].DeepClone(),
                        ["operator"] = operation,
                        ["value"] = valueToken
                    });
                }
                Filter = new JObject { ["conditions"] = conditions };
                DialogResult = DialogResult.OK;
            }
            catch (Exception error)
            {
                MessageBox.Show(this, error.Message, "筛选条件错误");
            }
        }
    }
}
