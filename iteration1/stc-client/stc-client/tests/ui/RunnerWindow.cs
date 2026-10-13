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
        private readonly string caseDirectory;
        private readonly DataGridView cases = Grid();
        private readonly DataGridView progress = Grid();
        private readonly TextBox details = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
        private readonly TextBox config = new TextBox { Width = 600 };
        private readonly TextBox user = new TextBox { Width = 130, Text = "localqa" };
        private readonly TextBox password = new TextBox { Width = 130, UseSystemPasswordChar = true };
        private readonly TextBox security = new TextBox { Width = 120, Text = "240210.IB" };
        private readonly NumericUpDown operationInterval = new NumericUpDown
        {
            Name = "OperationIntervalSeconds", Width = 95, Minimum = 0, Maximum = 60,
            DecimalPlaces = 1, Increment = 0.1m, Value = 0
        };
        private readonly Label status = new Label { AutoSize = true, Text = "就绪；请使用专用测试账号。" };
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
                    List<CaseDefinition> selected = Directory.GetFiles(source, "*.json").Select(CaseDefinition.Read)
                        .Where(item => ids.Contains(item.Id) || ids.Contains("all")).ToList();
                    SuiteRunner cli = new SuiteRunner();
                    if (options.ContainsKey("--interval-ms"))
                    {
                        cli.OperationIntervalMilliseconds = int.Parse(options["--interval-ms"], System.Globalization.CultureInfo.InvariantCulture);
                    }
                    return cli.RunAsync(selected, options["--config"], Environment.GetEnvironmentVariable("STC_UI_USER"),
                        Environment.GetEnvironmentVariable("STC_UI_PASSWORD"), Environment.GetEnvironmentVariable("STC_UI_SECURITY") ?? "240210.IB",
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
            caseDirectory = source;
            Directory.CreateDirectory(source);
            Text = "STC 销售交易 · UI 端到端测试管理";
            Width = 1360;
            Height = 900;
            Font = new Font("Microsoft YaHei UI", 10);
            StartPosition = FormStartPosition.CenterScreen;
            TableLayoutPanel root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 1, Padding = new Padding(10) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 115));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
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
            settings.Controls.Add(new Label { Text = "连接真实服务；每例独立登录；小批量串行执行。", AutoSize = true });
            settings.SetFlowBreak(settings.Controls[settings.Controls.Count - 1], true);
            settings.Controls.Add(new Label { Text = "操作间隔（秒）", AutoSize = true });
            settings.Controls.Add(operationInterval);
            settings.Controls.Add(new Label { Text = "0 = 不额外等待；运行中修改立即影响当前等待；按用例动作间隔。", AutoSize = true });
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
            FlowLayoutPanel commands = new FlowLayoutPanel { Dock = DockStyle.Fill };
            AddButton(commands, "新增用例", () => Edit(false));
            AddButton(commands, "编辑用例", () => Edit(true));
            AddButton(commands, "复制用例", Duplicate);
            AddButton(commands, "删除用例", Delete);
            AddButton(commands, "刷新", LoadCases);
            AddButton(commands, "全选", () => { foreach (DataGridViewRow row in cases.Rows) row.Cells[0].Value = true; });
            AddButton(commands, "运行选中", async () => await RunAsync());
            AddButton(commands, "停止并清理", () => runner?.Stop(), false);
            AddButton(commands, "打开本轮报告", () => OpenDirectory(report), false);
            AddButton(commands, "历史报告", () => OpenDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui-test-reports")), false);
            root.Controls.Add(commands, 0, 1);
            cases.Columns.Add(new DataGridViewCheckBoxColumn { Name = "run", HeaderText = "运行", FillWeight = 30 });
            cases.Columns.Add("id", "用例 ID");
            cases.Columns.Add("name", "用例名称");
            cases.Columns.Add("steps", "步骤数");
            cases.Columns.Add("state", "状态");
            cases.Columns.Add("description", "说明 / 前置条件");
            for (int i = 1; i < cases.Columns.Count; i++) cases.Columns[i].ReadOnly = true;
            cases.Columns[3].FillWeight = 35;
            root.Controls.Add(cases, 0, 2);
            SplitContainer bottom = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterDistance = 220 };
            foreach (string column in new[] { "用例", "步骤", "状态", "详情" }) progress.Columns.Add(column, column);
            progress.ReadOnly = true;
            bottom.Panel1.Controls.Add(progress);
            bottom.Panel2.Controls.Add(details);
            root.Controls.Add(bottom, 0, 3);
            root.Controls.Add(status, 0, 4);
            Controls.Add(root);
            progress.SelectionChanged += (sender, args) => details.Text = Convert.ToString(progress.CurrentRow?.Cells[3].Value);
            cases.CellDoubleClick += (sender, args) => { if (!running && args.RowIndex >= 0) Edit(true); };
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
            cases.Rows.Clear();
            foreach (string path in Directory.GetFiles(caseDirectory, "*.json").OrderBy(file => file))
            {
                try
                {
                    CaseDefinition item = CaseDefinition.Read(path);
                    int index = cases.Rows.Add(true, item.Id, item.Name, item.Steps.Count, "待运行", item.Description);
                    cases.Rows[index].Tag = path;
                }
                catch (Exception error)
                {
                    int index = cases.Rows.Add(false, Path.GetFileName(path), "无效用例", 0, "错误", error.Message);
                    cases.Rows[index].Tag = path;
                }
            }
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
                Steps = new List<TestStep> { new TestStep { Name = "打开手工报价", Action = "manual.open" } }
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
            List<CaseDefinition> selected = cases.Rows.Cast<DataGridViewRow>()
                .Where(row => (bool?)row.Cells[0].Value == true).Select(row => CaseDefinition.Read((string)row.Tag)).ToList();
            runner = new SuiteRunner { OperationIntervalMilliseconds = (int)(operationInterval.Value * 1000) };
            report = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ui-test-reports", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
            progress.Rows.Clear();
            running = true;
            foreach (Button button in lockedButtons) button.Enabled = false;
            cases.Enabled = false;
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
            }
        }

        private static void OpenDirectory(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
        }
    }
}
