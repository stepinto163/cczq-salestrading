using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.FrontendLearning {
    // All fields/operators come from the server contract, including external-data placeholders.
    internal sealed class ProductFilterForm : Form {
        readonly DataGridView grid = new DataGridView();
        readonly JArray catalog;
        internal JObject Filter { get; private set; }

        internal ProductFilterForm(JArray fields, JObject filter) {
            catalog = fields;
            Text = "完整条件筛选：报价 / 债券 / 流动性 / 信用 / 条款 / 区域";
            Width = 900;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;
            grid.Dock = DockStyle.Fill;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "field", HeaderText = "条件", ReadOnly = true });
            grid.Columns.Add(new DataGridViewComboBoxColumn { Name = "operator", HeaderText = "运算符" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "value", HeaderText = "值（留空不限；范围/多选用逗号分隔）" });
            foreach (JObject field in fields) {
                int index = grid.Rows.Add();
                var row = grid.Rows[index];
                row.Tag = field;
                row.Cells[0].Value = (string)field["group"] + " / " + ((string)field["label"] ?? (string)field["field"]);
                var operators = (DataGridViewComboBoxCell)row.Cells[1];
                operators.Items.AddRange(((JArray)field["operators"]).Values<string>().Cast<object>().ToArray());
                var condition = ((JArray)filter["conditions"]).OfType<JObject>().FirstOrDefault(c => (string)c["field"] == (string)field["field"]);
                row.Cells[1].Value = (string)condition?["operator"] ?? "EQ";
                row.Cells[2].Value = condition?["value"] is JArray values ? string.Join(",", values.Values<string>()) : (string)condition?["value"];
                row.Cells[2].ToolTipText = field["type"] + "; " + field["field"] + "; 布尔值使用 true/false；参考值："
                    + string.Join(", ", ((JArray)field["examples"] ?? new JArray()).Values<string>())
                    + ((bool?)field["includesSimulatedData"] == true ? "（含模拟资料）" : "");
            }
            var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 60 };
            var apply = new Button { Text = "应用", AutoSize = true };
            apply.Click += (sender, args) => Apply();
            var clear = new Button { Text = "清除全部", AutoSize = true };
            clear.Click += (sender, args) => {
                foreach (DataGridViewRow row in grid.Rows) row.Cells[2].Value = "";
            };
            bar.Controls.Add(apply);
            bar.Controls.Add(clear);
            bar.Controls.Add(new Label { Text = "BETWEEN=范围，IN=多选，CONTAINS=文本包含；缺失外部资料可使用模拟值", AutoSize = true });
            Controls.Add(grid);
            Controls.Add(bar);
        }

        void Apply() {
            try {
                grid.EndEdit();
                var conditions = new JArray();
                foreach (DataGridViewRow row in grid.Rows) {
                    string text = Convert.ToString(row.Cells[2].Value).Trim();
                    if (text.Length == 0) continue;
                    var field = (JObject)row.Tag;
                    string operation = (string)row.Cells[1].Value;
                    var parts = text.Split(new[] { ',', '，' }).Select(value => value.Trim()).ToArray();
                    Func<string, JToken> parse = value => {
                        if (operation == "EXISTS" || (string)field["type"] == "boolean") return new JValue(bool.Parse(value));
                        if ((string)field["type"] == "decimal") decimal.Parse(value, CultureInfo.InvariantCulture);
                        if (string.IsNullOrWhiteSpace(value)) throw new FormatException("条件值不能为空");
                        return new JValue(value);
                    };
                    if (operation == "BETWEEN" && (parts.Length != 2 || decimal.Parse(parts[0], CultureInfo.InvariantCulture) > decimal.Parse(parts[1], CultureInfo.InvariantCulture)))
                        throw new FormatException("范围请输入下限,上限，并且下限不能大于上限");
                    JToken valueToken = operation == "IN" || operation == "BETWEEN" ? new JArray(parts.Select(parse)) : parse(text);
                    conditions.Add(new JObject { ["field"] = field["field"].DeepClone(), ["operator"] = operation, ["value"] = valueToken });
                }
                Filter = new JObject { ["conditions"] = conditions };
                DialogResult = DialogResult.OK;
            } catch (Exception error) {
                MessageBox.Show(this, error.Message, "筛选条件错误");
            }
        }
    }
}
