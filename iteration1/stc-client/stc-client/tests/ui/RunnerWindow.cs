using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests
{
    internal sealed class RunnerWindow : Form
    {
        private const string DefaultUser = "test2";
        private const string DefaultPassword = "123456";
        private readonly string caseDirectory;
        private readonly DataGridView cases = Grid();
        private readonly TreeView pageTree = new TreeView { Dock = DockStyle.Fill, HideSelection = false };
        private readonly DataGridView progress = Grid();
        private readonly TextBox details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
        private readonly TextBox config = new TextBox { Width = 600 };
        private readonly TextBox user = new TextBox { Width = 130, Text = DefaultUser };
        private readonly TextBox password = new TextBox { Width = 130, Text = DefaultPassword, UseSystemPasswordChar = true };
        private readonly TextBox security = new TextBox { Width = 120, Text = "240210.IB" };
        private readonly NumericUpDown operationInterval = new NumericUpDown
        {
            Name = "OperationIntervalSeconds", Width = 95, Minimum = 0, Maximum = 60,
            DecimalPlaces = 1, Increment = 0.1m, Value = 2
        };
        private readonly Label status = new Label { AutoSize = true, Text = "就绪；请使用专用测试账号。" };
        private readonly Label selectionCount = new Label { AutoSize = true, Margin = new Padding(8, 8, 0, 0) };
        private readonly List<Button> lockedButtons = new List<Button>();
        private SuiteRunner runner;
        private bool running;
        private string report;

        [STAThread]
        public static int Main(string[] args)
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            for (int i = 0; i + 1 < args.Length; i += 2) options[args[i]] = args[i + 1];
            string source = options.ContainsKey("--cases") ? Path.GetFullPath(options["--cases"]) : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui-test-cases");
            if (options.ContainsKey("--run"))
            {
                try
                {
                    string[] ids = options["--run"].Split(',');
                    List<CaseDefinition> available = Directory.GetFiles(source, "*.json").Select(CaseDefinition.Read).ToList();
                    if (available.GroupBy(item => item.Id).Any(group => group.Count() > 1))
                    {
                        throw new InvalidOperationException("用例 ID 重复，请先修正目录。");
                    }
                    string[] unknown = ids.Where(id => id != "all" && !available.Any(item => item.Id == id)).ToArray();
                    if (unknown.Length > 0) throw new InvalidOperationException("未知用例 ID：" + string.Join(", ", unknown));
                    List<CaseDefinition> selected = available.Where(item => ids.Contains(item.Id) || ids.Contains("all"))
                        .OrderBy(item => item.Page, StringComparer.Ordinal).ThenBy(item => item.Feature, StringComparer.Ordinal)
                        .ThenBy(item => item.Id, StringComparer.Ordinal).ToList();
                    if (selected.Count == 0) throw new InvalidOperationException("没有可执行用例。");
                    SuiteRunner cli = new SuiteRunner();
                    if (options.ContainsKey("--interval-ms"))
                    {
                        cli.OperationIntervalMilliseconds = int.Parse(options["--interval-ms"], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    string cliUser = Environment.GetEnvironmentVariable("STC_UI_USER");
                    string cliPassword = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
                    return cli.RunAsync(selected, options["--config"], string.IsNullOrEmpty(cliUser) ? DefaultUser : cliUser,
                        string.IsNullOrEmpty(cliPassword) ? DefaultPassword : cliPassword, Environment.GetEnvironmentVariable("STC_UI_SECURITY") ?? "240210.IB",
                        options["--report"]).GetAwaiter().GetResult() ? 0 : 1;
                }
                catch (Exception error)
                {
                    if (options.ContainsKey("--report"))
                    {
                        Directory.CreateDirectory(options["--report"]);
                        File.WriteAllText(Path.Combine(options["--report"], "runner-error.txt"), error.ToString());
                    }
                    return 2;
                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RunnerWindow(source, options.ContainsKey("--config") ? options["--config"] : null));
            return 0;
        }

        public RunnerWindow(string source, string configFile = null)
        {
            // 与命令行执行使用相同的登录配置；普通启动仍默认 test2。
            string configuredUser = Environment.GetEnvironmentVariable("STC_UI_USER");
            string configuredPassword = Environment.GetEnvironmentVariable("STC_UI_PASSWORD");
            if (!string.IsNullOrWhiteSpace(configuredUser))
            {
                user.Text = configuredUser;
            }
            if (!string.IsNullOrEmpty(configuredPassword))
            {
                password.Text = configuredPassword;
            }
            caseDirectory = source;
            Directory.CreateDirectory(source);
            Text = "STC 销售交易 · UI 端到端测试管理";
            Width = 1360;
            Height = 900;
            Font = new Font("Microsoft YaHei UI", 10);
            StartPosition = FormStartPosition.CenterScreen;
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 1, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 115));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            FlowLayoutPanel settings = new FlowLayoutPanel { Dock = DockStyle.Fill };
            settings.Controls.Add(new Label { Text = "STC 配置", AutoSize = true });
            config.Text = configFile ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "STC.exe.config");
            settings.Controls.Add(config);
            AddButton(settings, "选择配置", () =>
            {
                using (OpenFileDialog dialog = new OpenFileDialog { Filter = "STC 配置|*.config|所有文件|*.*" })
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK) config.Text = dialog.FileName;
                }
            });
            settings.SetFlowBreak(settings.Controls[settings.Controls.Count - 1], true);
            foreach (var setting in new[] { Tuple.Create("账号", user), Tuple.Create("密码", password), Tuple.Create("测试债券", security) })
            {
                settings.Controls.Add(new Label { Text = setting.Item1, AutoSize = true });
                settings.Controls.Add(setting.Item2);
            }
            settings.Controls.Add(new Label
            {
                Text = "QT用例以登录用户收件；发布用例会真实发送；每例独立登录，串行执行。",
                AutoSize = true
            });
            settings.SetFlowBreak(settings.Controls[settings.Controls.Count - 1], true);
            settings.Controls.Add(new Label { Text = "操作间隔（秒）", AutoSize = true });
            settings.Controls.Add(operationInterval);
            settings.Controls.Add(new Label { Text = "每次点击、输入、选择前等待；0 = 不等待；运行中修改立即生效。", AutoSize = true });
            operationInterval.ValueChanged += (sender, args) =>
            {
                try
                {
                    if (runner != null)
                    {
                        runner.OperationIntervalMilliseconds = (int)(operationInterval.Value * 1000);
                    }
                }
                catch (Exception error)
                {
                    status.Text = "操作间隔更新失败：" + error.Message;
                }
            };
            root.Controls.Add(settings, 0, 0);
            FlowLayoutPanel commands = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, AutoScroll = true };
            AddButton(commands, "新增用例", () => Edit(false));
            AddButton(commands, "编辑用例", () => Edit(true));
            AddButton(commands, "复制用例", Duplicate);
            AddButton(commands, "删除用例", Delete);
            AddButton(commands, "刷新", LoadCases);
            AddButton(commands, "全选", () => SetAllCasesChecked(true));
            AddButton(commands, "取消全选", () => SetAllCasesChecked(false));
            AddButton(commands, "运行勾选", async () => await RunAsync());
            AddButton(commands, "停止并清理", () => runner?.Stop(), false);
            AddButton(commands, "打开本轮报告", () => OpenDirectory(report), false);
            AddButton(commands, "历史报告", () => OpenDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui-test-reports")), false);
            root.Controls.Add(commands, 0, 1);
            root.Controls.Add(selectionCount, 0, 2);
            cases.Columns.Add(new DataGridViewCheckBoxColumn { Name = "run", HeaderText = "运行", FillWeight = 30 });
            cases.Columns.Add("id", "用例 ID");
            cases.Columns.Add("name", "用例名称");
            cases.Columns.Add("steps", "步骤数");
            cases.Columns.Add("state", "状态");
            cases.Columns.Add("description", "说明 / 前置条件");
            cases.Columns.Add("page", "页面 / 功能");
            cases.Columns.Add("priority", "优先级");
            cases.MultiSelect = true;
            cases.CurrentCellDirtyStateChanged += (sender, args) =>
            {
                if (cases.IsCurrentCellDirty && cases.CurrentCell is DataGridViewCheckBoxCell)
                    cases.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };
            cases.CellValueChanged += (sender, args) => { if (args.ColumnIndex == 0) UpdateSelectionCount(); };
            cases.KeyDown += (sender, args) =>
            {
                if (running || args.KeyCode != Keys.Space || cases.SelectedRows.Count == 0) return;
                cases.EndEdit();
                DataGridViewRow[] selectedRows = cases.SelectedRows.Cast<DataGridViewRow>().Where(row => row.Visible).ToArray();
                bool check = selectedRows.Any(row => !IsCaseChecked(row));
                foreach (DataGridViewRow row in selectedRows) row.Cells[0].Value = check;
                args.Handled = true;
                args.SuppressKeyPress = true;
            };
            for (int i = 1; i < cases.Columns.Count; i++) cases.Columns[i].ReadOnly = true;
            cases.Columns[3].FillWeight = 35;
            cases.Columns["name"].FillWeight = 180;
            cases.Columns["description"].FillWeight = 200;
            cases.Columns["priority"].FillWeight = 40;
            SplitContainer catalog = new SplitContainer
            {
                Dock = DockStyle.Fill, Size = new Size(1300, 250),
                FixedPanel = FixedPanel.Panel1, SplitterDistance = 260
            };
            catalog.Panel1.Controls.Add(pageTree);
            catalog.Panel2.Controls.Add(cases);
            pageTree.AfterSelect += (sender, args) => ApplyPageFilter();
            root.Controls.Add(catalog, 0, 3);
            SplitContainer bottom = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 220 };
            foreach (string column in new[] { "用例", "步骤", "状态", "详情" }) progress.Columns.Add(column, column);
            progress.ReadOnly = true;
            bottom.Panel1.Controls.Add(progress);
            bottom.Panel2.Controls.Add(details);
            root.Controls.Add(bottom, 0, 4);
            root.Controls.Add(status, 0, 5);
            Controls.Add(root);
            progress.SelectionChanged += (sender, args) => details.Text = Convert.ToString(progress.CurrentRow?.Cells[3].Value);
            cases.CellDoubleClick += (sender, args) => { if (!running && args.RowIndex >= 0 && args.ColumnIndex > 0) Edit(true); };
            FormClosing += (sender, args) =>
            {
                if (!running) return;
                runner.Stop();
                status.Text = "正在停止并清理，请等待完成后关闭窗口。";
                args.Cancel = true;
            };
            Shown += (sender, args) => LoadCases();
        }

        private static DataGridView Grid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
                BackgroundColor = Color.White, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            };
        }

        private void AddButton(Control parent, string caption, Action action, bool locked = true)
        {
            Button button = new Button { Text = caption, AutoSize = true, Height = 32 };
            button.Click += (sender, args) =>
            {
                try { action(); }
                catch (Exception error) { MessageBox.Show(this, error.Message, "操作失败"); }
            };
            parent.Controls.Add(button);
            if (locked) lockedButtons.Add(button);
        }

        private void LoadCases()
        {
            string selectedPage = pageTree.SelectedNode?.Tag as string ?? "";
            cases.Rows.Clear();
            pageTree.Nodes.Clear();
            TreeNode allPages = pageTree.Nodes.Add("全部页面");
            allPages.Tag = "";
            var catalogItems = new List<Tuple<string, CaseDefinition>>();
            foreach (string path in Directory.GetFiles(caseDirectory, "*.json").OrderBy(file => file))
            {
                try
                {
                    CaseDefinition item = CaseDefinition.Read(path);
                    catalogItems.Add(Tuple.Create(path, item));
                }
                catch (Exception error)
                {
                    int index = cases.Rows.Add(false, Path.GetFileName(path), "无效用例", 0, "错误", error.Message);
                    cases.Rows[index].Tag = path;
                }
            }
            foreach (var entry in catalogItems.OrderBy(item => item.Item2.Page, StringComparer.Ordinal)
                .ThenBy(item => item.Item2.Feature, StringComparer.Ordinal).ThenBy(item => item.Item2.Id, StringComparer.Ordinal))
            {
                CaseDefinition item = entry.Item2;
                string category = item.Page + "/" + item.Feature;
                int index = cases.Rows.Add(true, item.Id, item.Name, item.Steps.Count, "待运行", item.Description, category, item.Priority);
                cases.Rows[index].Tag = entry.Item1;
                TreeNode parent = allPages;
                string prefix = "";
                foreach (string part in category.Split('/'))
                {
                    prefix = prefix.Length == 0 ? part : prefix + "/" + part;
                    TreeNode child = parent.Nodes.Cast<TreeNode>().FirstOrDefault(node => (string)node.Tag == prefix);
                    if (child == null)
                    {
                        child = parent.Nodes.Add(part);
                        child.Tag = prefix;
                    }
                    parent = child;
                }
            }
            allPages.Expand();
            pageTree.SelectedNode = FindPageNode(allPages, selectedPage) ?? allPages;
            ApplyPageFilter();
            UpdateSelectionCount();
        }

        private static TreeNode FindPageNode(TreeNode root, string path)
        {
            if ((string)root.Tag == path) return root;
            foreach (TreeNode child in root.Nodes)
            {
                TreeNode match = FindPageNode(child, path);
                if (match != null) return match;
            }
            return null;
        }

        private void ApplyPageFilter()
        {
            cases.EndEdit();
            cases.CurrentCell = null;
            string prefix = pageTree.SelectedNode?.Tag as string ?? "";
            foreach (DataGridViewRow row in cases.Rows)
            {
                string category = Convert.ToString(row.Cells["page"].Value);
                row.Visible = prefix.Length == 0 || category == prefix || category.StartsWith(prefix + "/", StringComparison.Ordinal);
                if (!row.Visible) row.Selected = false;
            }
            UpdateSelectionCount();
        }

        private static bool IsCaseChecked(DataGridViewRow row)
        {
            return (bool?)row.Cells[0].Value == true;
        }

        private void SetAllCasesChecked(bool check)
        {
            // 先提交当前复选框编辑，避免离开单元格时覆盖全选结果。
            cases.EndEdit();
            foreach (DataGridViewRow row in cases.Rows.Cast<DataGridViewRow>().Where(row => row.Visible)) row.Cells[0].Value = check;
            cases.ClearSelection();
            if (check)
            {
                foreach (DataGridViewRow row in cases.Rows.Cast<DataGridViewRow>().Where(row => row.Visible))
                {
                    row.Selected = true;
                }
            }
            UpdateSelectionCount();
        }

        private void UpdateSelectionCount()
        {
            var visibleRows = cases.Rows.Cast<DataGridViewRow>().Where(row => row.Visible).ToArray();
            selectionCount.Text = "当前范围已勾选 " + visibleRows.Count(IsCaseChecked)
                + " / " + visibleRows.Length + "（总计 " + cases.Rows.Count + "）；全选和运行仅作用于当前页面范围";
        }

        private string SelectedPath()
        {
            if (cases.CurrentRow == null) throw new InvalidOperationException("请先选择一个用例。");
            return (string)cases.CurrentRow.Tag;
        }

        private void Edit(bool existing)
        {
            string path = existing ? SelectedPath() : null;
            CaseDefinition original = existing ? CaseDefinition.Read(path) : new CaseDefinition
            {
                Id = "case-" + DateTime.Now.ToString("HHmmss"), Name = "新测试用例",
                Steps = new List<TestStep> { new TestStep { Name = "在报价管理选择本例报价", Action = "management.select" } }
            };
            using (CaseEditor editor = new CaseEditor(original, existing))
            {
                if (editor.ShowDialog(this) != DialogResult.OK) return;
                string destination = path ?? Path.Combine(caseDirectory, editor.Result.Id + ".json");
                if (!existing && File.Exists(destination)) throw new InvalidOperationException("用例 ID 已存在。");
                editor.Result.Save(destination);
            }
            LoadCases();
        }

        private void Duplicate()
        {
            CaseDefinition item = CaseDefinition.Read(SelectedPath());
            item.Id += "-copy-" + DateTime.Now.ToString("HHmmss");
            item.Name += "（副本）";
            using (CaseEditor editor = new CaseEditor(item, false))
            {
                if (editor.ShowDialog(this) != DialogResult.OK) return;
                string destination = Path.Combine(caseDirectory, editor.Result.Id + ".json");
                if (File.Exists(destination)) throw new InvalidOperationException("用例 ID 已存在。");
                editor.Result.Save(destination);
            }
            LoadCases();
        }

        private void Delete()
        {
            string path = SelectedPath();
            if (MessageBox.Show(this, "删除用例 " + Path.GetFileName(path) + "？历史报告保留。", "删除用例", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            File.Delete(path);
            LoadCases();
        }

        private async System.Threading.Tasks.Task RunAsync()
        {
            if (running) return;
            cases.EndEdit();
            List<CaseDefinition> selected;
            try
            {
                selected = cases.Rows.Cast<DataGridViewRow>()
                    .Where(row => row.Visible && IsCaseChecked(row)).Select(row => CaseDefinition.Read((string)row.Tag)).ToList();
                if (selected.GroupBy(item => item.Id).Any(group => group.Count() > 1))
                {
                    throw new InvalidOperationException("选中用例的 ID 重复，请先修正。");
                }
            }
            catch (Exception error)
            {
                status.Text = "用例配置错误：" + error.Message;
                return;
            }
            if (selected.Count == 0)
            {
                status.Text = "请先勾选要运行的用例；Ctrl/Shift 多选后按空格可批量勾选。";
                return;
            }
            runner = new SuiteRunner { OperationIntervalMilliseconds = (int)(operationInterval.Value * 1000) };
            report = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui-test-reports", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            progress.Rows.Clear();
            running = true;
            foreach (Button button in lockedButtons) button.Enabled = false;
            cases.Enabled = false;
            pageTree.Enabled = false;
            runner.Progress += entry =>
            {
                string state = (string)entry["state"];
                int index = progress.Rows.Add((string)entry["caseId"], (string)entry["step"], state, (string)entry["detail"]);
                progress.Rows[index].DefaultCellStyle.ForeColor = state == "FAILED" ? Color.Firebrick : state == "PASSED" ? Color.DarkGreen : Color.Black;
                progress.FirstDisplayedScrollingRowIndex = index;
                foreach (DataGridViewRow row in cases.Rows)
                {
                    if ((string)row.Cells[1].Value == (string)entry["caseId"]) row.Cells[4].Value = state + " · " + entry["step"];
                }
                status.Text = entry["caseId"] + " / " + entry["step"] + " / " + state;
            };
            try
            {
                bool passed = await runner.RunAsync(selected, config.Text, user.Text, password.Text, security.Text, report);
                status.Text = (passed ? "全部通过" : "存在失败、取消或未运行用例") + "；报告：" + report;
            }
            catch (Exception error)
            {
                status.Text = "运行失败：" + error.Message;
                MessageBox.Show(this, error.ToString(), "运行失败");
            }
            finally
            {
                running = false;
                foreach (Button button in lockedButtons) button.Enabled = true;
                cases.Enabled = true;
                pageTree.Enabled = true;
            }
        }

        private static void OpenDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
