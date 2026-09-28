using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests
{
    internal sealed class CaseEditor : Form
    {
        private readonly TextBox id = new TextBox();
        private readonly TextBox name = new TextBox();
        private readonly TextBox description = new TextBox();
        private readonly ListBox steps = new ListBox();
        private readonly TextBox stepName = new TextBox();
        private readonly ComboBox action = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly DataGridView parameters = new DataGridView
        {
            Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false
        };
        private readonly List<TestStep> items;
        private int editing = -1;
        private bool loading;
        public CaseDefinition Result { get; private set; }

        public CaseEditor(CaseDefinition original, bool existing)
        {
            Result = JsonConvert.DeserializeObject<CaseDefinition>(JsonConvert.SerializeObject(original));
            items = Result.Steps;
            Text = existing ? "编辑测试用例" : "新增测试用例";
            Width = 1050;
            Height = 760;
            StartPosition = FormStartPosition.CenterParent;
            Font = new System.Drawing.Font("Microsoft YaHei UI", 10);
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 6, Padding = new Padding(12) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (int i = 0; i < 3; i++) root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            AddField(root, "用例 ID（保存后固定）", id, 0);
            AddField(root, "用例名称", name, 1);
            AddField(root, "说明 / 前置条件", description, 2);
            id.Text = Result.Id;
            id.ReadOnly = existing;
            name.Text = Result.Name;
            description.Text = Result.Description;
            Label hint = new Label { Text = Actions.Hint, Dock = DockStyle.Fill };
            root.Controls.Add(hint, 0, 3);
            root.SetColumnSpan(hint, 2);
            Panel left = new Panel { Dock = DockStyle.Fill };
            FlowLayoutPanel stepButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 80 };
            AddButton(stepButtons, "新增步骤", () =>
            {
                StoreStep();
                items.Add(new TestStep { Name = "新步骤", Action = "manual.open" });
                RefreshSteps(items.Count - 1);
            });
            AddButton(stepButtons, "删除步骤", () =>
            {
                if (editing < 0) return;
                items.RemoveAt(editing);
                editing = -1;
                RefreshSteps(Math.Max(0, steps.SelectedIndex - 1));
            });
            AddButton(stepButtons, "上移", () => MoveStep(-1));
            AddButton(stepButtons, "下移", () => MoveStep(1));
            steps.Dock = DockStyle.Fill;
            left.Controls.Add(steps);
            left.Controls.Add(stepButtons);
            root.Controls.Add(left, 0, 4);
            TableLayoutPanel details = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(10, 0, 0, 0) };
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            details.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
            details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            stepName.Dock = DockStyle.Fill;
            action.Dock = DockStyle.Fill;
            action.DisplayMember = "Value";
            action.ValueMember = "Key";
            action.DataSource = Actions.Labels.ToList();
            parameters.Columns.Add("key", "参数");
            parameters.Columns[0].ReadOnly = true;
            parameters.Columns.Add("value", "值（可编辑）");
            details.Controls.Add(stepName, 0, 0);
            details.Controls.Add(action, 0, 1);
            details.Controls.Add(parameters, 0, 2);
            root.Controls.Add(details, 1, 4);
            FlowLayoutPanel footer = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            AddButton(footer, "保存", () =>
            {
                try
                {
                    StoreStep();
                    Result.Id = id.Text.Trim();
                    Result.Name = name.Text.Trim();
                    Result.Description = description.Text.Trim();
                    Result.Validate();
                    DialogResult = DialogResult.OK;
                    Close();
                }
                catch (Exception error) { MessageBox.Show(this, error.Message, "无法保存"); }
            });
            AddButton(footer, "取消", () => { DialogResult = DialogResult.Cancel; Close(); });
            root.Controls.Add(footer, 0, 5);
            root.SetColumnSpan(footer, 2);
            Controls.Add(root);
            steps.SelectedIndexChanged += (sender, args) =>
            {
                if (loading) return;
                StoreStep();
                LoadStep(steps.SelectedIndex);
            };
            action.SelectedValueChanged += (sender, args) =>
            {
                if (loading || editing < 0) return;
                string key = (string)action.SelectedValue;
                items[editing].Action = key;
                LoadParameters(items[editing]);
            };
            RefreshSteps(0);
        }

        private static void AddField(TableLayoutPanel root, string label, TextBox input, int row)
        {
            root.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill }, 0, row);
            input.Dock = DockStyle.Fill;
            root.Controls.Add(input, 1, row);
        }

        private static void AddButton(Control parent, string caption, Action handler)
        {
            Button button = new Button { Text = caption, AutoSize = true };
            button.Click += (sender, args) => handler();
            parent.Controls.Add(button);
        }

        private void StoreStep()
        {
            if (editing < 0 || editing >= items.Count) return;
            parameters.EndEdit();
            TestStep step = items[editing];
            step.Name = stepName.Text.Trim();
            step.Action = (string)action.SelectedValue;
            step.Parameters = new JObject();
            foreach (DataGridViewRow row in parameters.Rows)
            {
                step.Parameters[(string)row.Cells[0].Value] = Convert.ToString(row.Cells[1].Value);
            }
        }

        private void LoadParameters(TestStep step)
        {
            parameters.Rows.Clear();
            foreach (string field in Actions.Fields[step.Action]) parameters.Rows.Add(field, (string)step.Parameters[field] ?? "");
        }

        private void LoadStep(int index)
        {
            editing = index;
            loading = true;
            try
            {
                if (index < 0 || index >= items.Count) return;
                TestStep step = items[index];
                stepName.Text = step.Name;
                action.SelectedValue = step.Action;
                LoadParameters(step);
            }
            finally { loading = false; }
        }

        private void RefreshSteps(int index)
        {
            loading = true;
            steps.Items.Clear();
            for (int i = 0; i < items.Count; i++) steps.Items.Add((i + 1) + ". " + items[i].Name);
            steps.SelectedIndex = items.Count == 0 ? -1 : Math.Min(index, items.Count - 1);
            loading = false;
            LoadStep(steps.SelectedIndex);
        }

        private void MoveStep(int direction)
        {
            if (editing < 0 || editing + direction < 0 || editing + direction >= items.Count) return;
            StoreStep();
            TestStep step = items[editing];
            int target = editing + direction;
            items.RemoveAt(editing);
            items.Insert(target, step);
            RefreshSteps(target);
        }
    }
}
