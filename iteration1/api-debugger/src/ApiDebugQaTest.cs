using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class ApiDebugForm {
        readonly ComboBox qaSecurity = new ComboBox { Width = 320, DropDownStyle = ComboBoxStyle.DropDown };
        readonly TextBox qaSecondUser = new TextBox { Width = 100 };
        readonly TextBox qaSecondPassword = new TextBox { Width = 110, UseSystemPasswordChar = true };
        readonly TextBox qaOutput = Editor(true);
        readonly Button qaRun = new Button { Text = "开始 QA 功能测试（3条报价）", AutoSize = true };
        readonly Button qaStop = new Button { Text = "停止并清理", AutoSize = true, Enabled = false };
        Process qaProcess;
        string qaReportPath;

        void InitializeQaTestTab(TabControl tabs) {
            var tab = new TabPage("QA 自动测试");
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var accounts = Bar();
            accounts.Controls.Add(Label("账号一使用顶部登录输入；账号二"));
            accounts.Controls.Add(qaSecondUser);
            accounts.Controls.Add(Label("密码"));
            accounts.Controls.Add(qaSecondPassword);
            layout.Controls.Add(accounts, 0, 0);
            var securities = Bar();
            securities.Controls.Add(Label("债券代码/名称"));
            securities.Controls.Add(qaSecurity);
            Button(securities, "查询基础资料", LoadQaSecurities);
            securities.Controls.Add(qaRun);
            securities.Controls.Add(qaStop);
            Button(securities, "打开报告", () => {
                if (qaReportPath == null) throw new InvalidOperationException("尚未运行测试。");
                Process.Start("explorer.exe", Path.GetDirectoryName(qaReportPath));
                return Task.CompletedTask;
            });
            layout.Controls.Add(securities, 0, 1);
            layout.Controls.Add(new Label {
                Dock = DockStyle.Fill, AutoSize = false,
                Text = "使用两个专用、非管理员 QA 账号。覆盖生命周期、查询、三类推送及四类提醒；只创建3条报价。\r\n所选债券须有 RATE/CREDIT 分类且没有有效行情；结束清理临时对象、恢复提醒开关，审计历史保留。"
            }, 0, 2);
            layout.Controls.Add(qaOutput, 0, 3);
            tab.Controls.Add(layout);
            tabs.TabPages.Add(tab);
            qaRun.Click += async (sender, args) => await Guard(RunQaTest);
            qaStop.Click += (sender, args) => RequestQaStop();
            FormClosing += (sender, args) => {
                if (qaProcess != null && !qaProcess.HasExited) {
                    RequestQaStop();
                    args.Cancel = true;
                    QaLine("正在清理测试数据，结束后可关闭窗口。");
                }
            };
        }

        async Task LoadQaSecurities() {
            if (!connected) throw new InvalidOperationException("请先使用顶部账号连接 GW。");
            string searchText = qaSecurity.Text.Split('|')[0].Trim();
            if (string.IsNullOrWhiteSpace(searchText)) {
                throw new InvalidOperationException("请先输入债券代码或名称关键词。");
            }
            JObject data = await Task.Run(() => client.Data("ficc.base.query", new JObject {
                ["kind"] = "SECURITY", ["text"] = searchText, ["limit"] = 30
            }));
            qaSecurity.Items.Clear();
            foreach (JObject bond in (JArray)data["records"]) {
                qaSecurity.Items.Add((string)bond["securityId"] + " | " + (string)bond["securityName"]);
            }
            if (qaSecurity.Items.Count > 0) qaSecurity.SelectedIndex = 0;
            QaLine("本次显示 " + qaSecurity.Items.Count + " 条基础资料；可按代码/名称继续查询，也可直接填写任意已加载债券代码。");
        }

        async Task RunQaTest() {
            if (qaProcess != null) throw new InvalidOperationException("测试已经在运行。");
            string securityId = qaSecurity.Text.Split('|')[0].Trim();
            if (string.IsNullOrWhiteSpace(securityId) || string.IsNullOrWhiteSpace(qaSecondUser.Text)
                || string.IsNullOrWhiteSpace(qaSecondPassword.Text)) {
                throw new InvalidOperationException("请填写债券代码及第二个真实 STC 账号和密码。");
            }
            var configuration = new JObject {
                ["host"] = host.Text, ["port"] = (int)port.Value,
                ["username"] = user.Text, ["password"] = password.Text,
                ["secondUsername"] = qaSecondUser.Text, ["secondPassword"] = qaSecondPassword.Text,
                ["securityId"] = securityId
            };
            string directory = Path.Combine(MessageLog.DirectoryPath, "qa-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(directory);
            qaReportPath = Path.Combine(directory, "report.json");
            qaOutput.Clear();
            await Disconnect(); // The child owns fresh user sessions; avoid replacing the parent's session silently.
            qaRun.Enabled = false;
            qaStop.Enabled = true;
            connect.Enabled = false;
            using (var process = new Process()) {
                bool started = false;
                process.StartInfo = new ProcessStartInfo(Application.ExecutablePath, "--qa-autotest \"" + qaReportPath + "\"") {
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
                };
                process.OutputDataReceived += (sender, args) => {
                    if (args.Data != null) QaLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) => {
                    if (args.Data != null) QaLine(args.Data);
                };
                try {
                    process.Start();
                    started = true;
                    qaProcess = process;
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    // Passwords travel over the local anonymous pipe, never command-line arguments or config files.
                    process.StandardInput.WriteLine(configuration.ToString(Newtonsoft.Json.Formatting.None));
                    process.StandardInput.Close();
                    bool completed = await Task.Run(() => process.WaitForExit(90000));
                    if (!completed) {
                        process.Kill();
                        await Task.Run(() => process.WaitForExit(5000));
                        QaLine("超过90秒，已终止测试。清理状态未知，请核对报告中记录的报价/分组和提醒开关备份。");
                        JObject incomplete = File.Exists(qaReportPath) ? JObject.Parse(File.ReadAllText(qaReportPath)) : new JObject();
                        incomplete["result"] = "ABORTED";
                        incomplete["cleanupStatus"] = "UNKNOWN";
                        File.WriteAllText(qaReportPath, incomplete.ToString());
                    } else if (File.Exists(qaReportPath)) {
                        JObject report = JObject.Parse(File.ReadAllText(qaReportPath));
                        if ((string)report["result"] == "RUNNING") {
                            report["result"] = "ABORTED";
                            report["cleanupStatus"] = "UNKNOWN";
                            report["exitCode"] = process.ExitCode;
                            File.WriteAllText(qaReportPath, report.ToString());
                            QaLine("测试进程提前退出，清理状态未知，请按报告核对。");
                        }
                        QaLine("结果：" + (string)report["result"]);
                        if (report["error"] != null) QaLine((string)report["error"]);
                        QaLine("清理问题：" + report["cleanupErrors"]);
                    } else {
                        QaLine("未生成报告，退出码：" + process.ExitCode);
                    }
                    QaLine("报告：" + qaReportPath);
                } finally {
                    if (started && !process.HasExited) process.Kill();
                    qaProcess = null;
                    qaRun.Enabled = true;
                    qaStop.Enabled = false;
                    connect.Enabled = true;
                    status.Text = "QA 测试结束；手动请求前请重新登录";
                }
            }
        }

        void RequestQaStop() {
            if (qaReportPath != null) File.WriteAllText(qaReportPath + ".cancel", "cancel");
            qaStop.Enabled = false;
            QaLine("已请求停止，将在当前请求结束后清理。");
        }
        void QaLine(string message) {
            Ui(() => qaOutput.AppendText(message + Environment.NewLine));
        }
    }
}
