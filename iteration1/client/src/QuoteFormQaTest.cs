using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Ficc.SalesTrading {
    public sealed partial class QuoteForm {
        readonly ComboBox testMode = Choice("STC QA（3条报价）", "本机模拟回归（8条报价）");
        readonly ComboBox qaSecurity = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDown };
        readonly TextBox qaSecondUser = new TextBox { Width = 100 };
        readonly TextBox qaSecondPassword = new TextBox { Width = 100, UseSystemPasswordChar = true };
        Process qaProcess;
        string qaReportPath;

        void InitializeQaTestSettings() {
            var settings = Bar();
            settings.Dock = DockStyle.Top;
            settings.Height = 42;
            testMode.Width = 175;
            settings.Controls.Add(Label("模式"));
            settings.Controls.Add(testMode);
            settings.Controls.Add(Label("账号二"));
            settings.Controls.Add(qaSecondUser);
            settings.Controls.Add(Label("密码"));
            settings.Controls.Add(qaSecondPassword);
            settings.Controls.Add(Label("债券"));
            settings.Controls.Add(qaSecurity);
            AddButton(settings, "查询基础资料", LoadQaSecurities);
            testTab.Controls.Add(settings);
            testStatus.Text = "QA 使用顶部 GW / 账号一及本页账号二；选择无有效行情的债券，覆盖四类提醒，最多3条报价。";
        }

        internal async Task RunSelectedAutoTests() {
            SetDiagnosticsVisible(true);
            tabs.SelectedTab = testTab;
            if (workspace.Height > 400) {
                workspace.SplitterDistance = Math.Max(150, workspace.Height - 360);
            }
            if (testMode.SelectedIndex == 1) {
                if (gatewayHost.Text != "127.0.0.1" || gatewayPort.Value != 18600) {
                    throw new InvalidOperationException("固定模拟回归仅用于本机 GW 127.0.0.1:18600；QA 请选择 STC QA 模式。");
                }
                await RunAutoTests();
                return;
            }
            using (var single = new Semaphore(1, 1, @"Local\FiccSalesTradingClient.AutoTests")) {
                if (!single.WaitOne(0)) {
                    throw new InvalidOperationException("另一轮自动测试正在运行。");
                }
                try {
                    LastAutoTestResult = null;
                    await RunQaTest();
                } finally {
                    single.Release();
                }
            }
        }

        async Task DisconnectForQaTest() {
            readRetryTimer.Stop();
            connected = false;
            notificationPreference = null;
            topic = null;
            streamId = null;
            viewId = null;
            priceTopic = null;
            priceStreamId = null;
            priceViewId = null;
            notificationTopic = null;
            notificationStreamId = null;
            notificationViewId = null;
            await Task.Run(() => client.Dispose());
            user.Enabled = true;
            gatewayHost.Enabled = true;
            gatewayPort.Enabled = true;
            loginPassword.Enabled = true;
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
            TestLine("本次显示 " + qaSecurity.Items.Count + " 条基础资料；可按代码/名称继续查询，也可直接填写任意已加载债券代码。");
        }

        async Task RunQaTest() {
            if (qaProcess != null) throw new InvalidOperationException("测试已经在运行。");
            string securityId = qaSecurity.Text.Split('|')[0].Trim();
            if (string.IsNullOrWhiteSpace(securityId) || string.IsNullOrWhiteSpace(qaSecondUser.Text)
                || string.IsNullOrWhiteSpace(qaSecondPassword.Text)) {
                throw new InvalidOperationException("请填写债券代码及第二个真实 STC 账号和密码。");
            }
            var configuration = new JObject {
                ["host"] = gatewayHost.Text, ["port"] = (int)gatewayPort.Value,
                ["username"] = user.Text, ["password"] = loginPassword.Text,
                ["secondUsername"] = qaSecondUser.Text, ["secondPassword"] = qaSecondPassword.Text,
                ["securityId"] = securityId
            };
            string directory = Path.Combine(MessageLog.DirectoryPath, "qa-" + DateTime.Now.ToString("yyyyMMdd-HHmmss"));
            Directory.CreateDirectory(directory);
            testDirectory = directory;
            openTestLogs.Enabled = true;
            qaReportPath = Path.Combine(directory, "report.json");
            testOutput.Clear();
            await DisconnectForQaTest(); // The child owns fresh user sessions; avoid replacing the parent's session silently.
            testMode.Enabled = false;
            stopTest.Enabled = true;
            using (var process = new Process()) {
                bool started = false;
                process.StartInfo = new ProcessStartInfo(Application.ExecutablePath, "--qa-autotest \"" + qaReportPath + "\"") {
                    WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                    UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
                };
                process.OutputDataReceived += (sender, args) => {
                    if (args.Data != null) TestLine(args.Data);
                };
                process.ErrorDataReceived += (sender, args) => {
                    if (args.Data != null) TestLine(args.Data);
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
                        TestLine("超过90秒，已终止测试。清理状态未知，请核对报告中记录的报价/分组和提醒开关备份。");
                        JObject incomplete = File.Exists(qaReportPath) ? JObject.Parse(File.ReadAllText(qaReportPath)) : new JObject();
                        incomplete["result"] = "ABORTED";
                        incomplete["cleanupStatus"] = "UNKNOWN";
                        File.WriteAllText(qaReportPath, incomplete.ToString());
                        LastAutoTestResult = incomplete;
                    } else if (File.Exists(qaReportPath)) {
                        JObject report = JObject.Parse(File.ReadAllText(qaReportPath));
                        if ((string)report["result"] == "RUNNING") {
                            report["result"] = "ABORTED";
                            report["cleanupStatus"] = "UNKNOWN";
                            report["exitCode"] = process.ExitCode;
                            File.WriteAllText(qaReportPath, report.ToString());
                            TestLine("测试进程提前退出，清理状态未知，请按报告核对。");
                        }
                        LastAutoTestResult = report;
                        testStatus.Text = "QA：" + (string)report["result"];
                        TestLine("结果：" + (string)report["result"]);
                        if (report["error"] != null) TestLine((string)report["error"]);
                        TestLine("清理问题：" + report["cleanupErrors"]);
                    } else {
                        TestLine("未生成报告，退出码：" + process.ExitCode);
                    }
                    TestLine("报告：" + qaReportPath);
                } finally {
                    if (started && !process.HasExited) process.Kill();
                    qaProcess = null;
                    testMode.Enabled = true;
                    stopTest.Enabled = false;
                    state.Text = "QA 测试结束；手动操作前请重新连接 GW";
                }
            }
        }

        void RequestQaStop() {
            if (qaReportPath != null) File.WriteAllText(qaReportPath + ".cancel", "cancel");
            stopTest.Enabled = false;
            TestLine("已请求停止，将在当前请求结束后清理。");
        }
    }
}
