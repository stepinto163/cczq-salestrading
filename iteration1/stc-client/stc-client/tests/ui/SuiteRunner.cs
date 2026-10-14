using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests
{
    internal sealed class SuiteRunner
    {
        public string OutputDirectory { get; private set; }
        public string CurrentDirectory { get; private set; }
        public bool StopRequested { get; private set; }
        public event Action<JObject> Progress;

        private int operationIntervalMilliseconds = 2000;
        private OperationWaitBudget operationWaitBudget;

        public int OperationIntervalMilliseconds
        {
            get { return operationIntervalMilliseconds; }
            set
            {
                if (value < 0 || value > OperationInterval.MaximumMilliseconds)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (CurrentDirectory != null)
                {
                    new OperationInterval(CurrentDirectory).Save(value);
                }
                operationIntervalMilliseconds = value;
            }
        }

        public void Stop()
        {
            StopRequested = true;
            if (CurrentDirectory != null) File.WriteAllText(Path.Combine(CurrentDirectory, "cancel"), "stop");
        }

        public async Task<bool> RunAsync(IEnumerable<CaseDefinition> selected, string configFile, string user, string password, string securityId, string output)
        {
            List<CaseDefinition> cases = selected.ToList();
            if (cases.Count == 0) throw new InvalidOperationException("请勾选用例。");
            foreach (CaseDefinition definition in cases) definition.Validate();
            ValidateQTradeEnvironment(cases, user);
            if (!File.Exists(configFile) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("请选择真实 STC 配置并填写专用测试账号和密码。");
            }
            XmlDocument config = new XmlDocument();
            config.Load(configFile);
            if (config.SelectSingleNode("/configuration/appSettings/add[@key='Ficc.IntegrationMode']")?.Attributes["value"]?.Value != "stc")
            {
                throw new InvalidOperationException("完整端到端测试要求 Ficc.IntegrationMode=stc，不能使用模拟服务配置。");
            }
            string runtime = AppDomain.CurrentDomain.BaseDirectory;
            OutputDirectory = Path.GetFullPath(output);
            Directory.CreateDirectory(OutputDirectory);
            List<JObject> results = new List<JObject>();
            string lockPath = Path.Combine(runtime, "ui-tests.lock");
            // File share lock is released even on process exit. A stale file is harmless.
            using (FileStream runLock = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                File.WriteAllText(Path.Combine(OutputDirectory, "suite-cases.json"), JsonConvert.SerializeObject(cases, Newtonsoft.Json.Formatting.Indented));
                try
                {
                    foreach (CaseDefinition definition in cases)
                    {
                        if (StopRequested)
                        {
                            results.Add(new JObject { ["caseId"] = definition.Id, ["name"] = definition.Name, ["status"] = "NOT_RUN", ["cleanupPassed"] = true });
                            continue;
                        }
                        CurrentDirectory = Path.Combine(OutputDirectory, definition.Id);
                        Directory.CreateDirectory(CurrentDirectory);
                        new OperationInterval(CurrentDirectory).Save(OperationIntervalMilliseconds);
                        operationWaitBudget = new OperationWaitBudget();
                        string caseFile = Path.Combine(CurrentDirectory, "case.json");
                        definition.Save(caseFile);
                        SetSetting(config, "IsSingle", "false");
                        SetSetting(config, "DataPath", Path.Combine(CurrentDirectory, "client-state"));
                        Directory.CreateDirectory(Path.Combine(CurrentDirectory, "client-state"));
                        SetSetting(config, "LogPath", Path.Combine(CurrentDirectory, "logs", "STC_"));
                        Directory.CreateDirectory(Path.Combine(CurrentDirectory, "logs"));
                        string worker = Path.Combine(runtime, "UiScenarioWorker.exe");
                        config.Save(worker + ".config");
                        ProcessStartInfo start = new ProcessStartInfo(worker, Quote(caseFile) + " " + Quote(CurrentDirectory))
                        {
                            WorkingDirectory = runtime, UseShellExecute = false,
                            RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true
                        };
                        start.EnvironmentVariables["STC_UI_USER"] = user;
                        start.EnvironmentVariables["STC_UI_PASSWORD"] = password;
                        start.EnvironmentVariables["STC_UI_SECURITY"] = securityId;
                        using (StreamWriter console = new StreamWriter(Path.Combine(CurrentDirectory, "console.log"), false, Encoding.UTF8))
                        using (Process process = new Process { StartInfo = start })
                        {
                            object outputLock = new object();
                            DataReceivedEventHandler record = (sender, args) =>
                            {
                                if (args.Data == null) return;
                                lock (outputLock) console.WriteLine(args.Data.Replace(password, "[REDACTED]"));
                            };
                            process.OutputDataReceived += record;
                            process.ErrorDataReceived += record;
                            process.Start();
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            Stopwatch elapsed = Stopwatch.StartNew();
                            DateTime? stopTime = null;
                            int eventCount = 0;
                            bool killed = false;
                            int timeoutSeconds = Math.Min(600, 120 + definition.Steps.Count * 25);
                            while (!process.HasExited)
                            {
                                ReadEvents(ref eventCount);
                                if (StopRequested && stopTime == null)
                                {
                                    stopTime = DateTime.UtcNow;
                                    File.WriteAllText(Path.Combine(CurrentDirectory, "cancel"), "stop");
                                }
                                if (elapsed.ElapsedMilliseconds - operationWaitBudget.ExcludedMilliseconds > timeoutSeconds * 1000L || (stopTime != null && DateTime.UtcNow - stopTime > TimeSpan.FromSeconds(30)))
                                {
                                    // This is the single worker started above, never an unrelated STC process.
                                    process.Kill();
                                    killed = true;
                                    break;
                                }
                                await Task.Delay(200);
                            }
                            process.WaitForExit();
                            ReadEvents(ref eventCount);
                            string resultFile = Path.Combine(CurrentDirectory, "result.json");
                            JObject result = File.Exists(resultFile) ? JObject.Parse(File.ReadAllText(resultFile)) : new JObject
                            {
                                ["caseId"] = definition.Id, ["name"] = definition.Name, ["status"] = "FAILED",
                                ["elapsedMillis"] = elapsed.ElapsedMilliseconds, ["cleanupPassed"] = false,
                                ["reason"] = killed ? "超时或停止宽限期结束；清理未确认，请检查 owned-quotes.json" : "进程提前退出，退出码 " + process.ExitCode + "；清理未确认"
                            };
                            if (process.ExitCode != 0 && (string)result["status"] == "PASSED") result["status"] = "FAILED";
                            File.WriteAllText(resultFile, result.ToString());
                            results.Add(result);
                            Progress?.Invoke(new JObject { ["caseId"] = definition.Id, ["step"] = "结果", ["state"] = result["status"], ["detail"] = result["reason"] ?? "" });
                            // Do not contaminate later cases when cleanup is uncertain.
                            if ((bool?)result["cleanupPassed"] != true) StopRequested = true;
                        }
                    }
                }
                finally
                {
                    CurrentDirectory = null;
                    File.WriteAllText(Path.Combine(OutputDirectory, "summary.json"), new JArray(results).ToString());
                    WriteJUnit(results);
                }
            }
            return results.Count == cases.Count && results.All(result => (string)result["status"] == "PASSED");
        }

        private void ReadEvents(ref int count)
        {
            string file = Path.Combine(CurrentDirectory, "events.jsonl");
            if (!File.Exists(file)) return;
            string content;
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(stream)) content = reader.ReadToEnd();
            string[] lines = content.Split('\n');
            while (count < lines.Length - 1)
            {
                string line = lines[count].Trim();
                if (line.Length > 0)
                {
                    JObject entry = JObject.Parse(line);
                    if ((string)entry["state"] == "INTERVAL_WAIT")
                    {
                        operationWaitBudget.Begin();
                    }
                    else if ((string)entry["state"] == "INTERVAL_DONE")
                    {
                        operationWaitBudget.Complete((long?)entry["waitMillis"] ?? 0);
                    }
                    Progress?.Invoke(entry);
                }
                count++;
            }
        }

        private void WriteJUnit(IEnumerable<JObject> results)
        {
            List<JObject> list = results.ToList();
            XElement suite = new XElement("testsuite", new XAttribute("name", "STC UI end-to-end"),
                new XAttribute("tests", list.Count), new XAttribute("failures", list.Count(r => (string)r["status"] == "FAILED")),
                new XAttribute("skipped", list.Count(r => (string)r["status"] == "NOT_RUN" || (string)r["status"] == "CANCELLED")));
            foreach (JObject result in list)
            {
                XElement test = new XElement("testcase", new XAttribute("classname", "SalesTrading"),
                    new XAttribute("name", (string)result["caseId"]), new XAttribute("time", ((double?)result["elapsedMillis"] ?? 0) / 1000));
                string status = (string)result["status"];
                if (status == "FAILED") test.Add(new XElement("failure", result.ToString()));
                else if (status != "PASSED") test.Add(new XElement("skipped", status));
                suite.Add(test);
            }
            new XDocument(suite).Save(Path.Combine(OutputDirectory, "junit.xml"));
        }

        private static void SetSetting(XmlDocument config, string key, string value)
        {
            XmlElement setting = (XmlElement)config.SelectSingleNode("/configuration/appSettings/add[@key='" + key + "']");
            if (setting == null)
            {
                setting = config.CreateElement("add");
                setting.SetAttribute("key", key);
                config.SelectSingleNode("/configuration/appSettings").AppendChild(setting);
            }
            setting.SetAttribute("value", value);
        }

        // 在启动客户端前检查真实 QT 用例的运行条件，避免登录后才逐例报同一配置错误。
        internal static void ValidateQTradeEnvironment(IEnumerable<CaseDefinition> cases, string user)
        {
            var steps = cases.SelectMany(definition => definition.Steps).ToList();
            var publishing = steps.Where(step => step.Action == "qtrade.publish").ToList();
            if (publishing.Count > 0 && Environment.GetEnvironmentVariable("STC_UI_QTRADE_PUBLISH") != "1")
            {
                throw new InvalidOperationException("所选用例包含真实 QTrade 发布，当前管理器为普通模式。请用 autotest.cmd restart -QTrade 启动 QT 模式，再运行用例；尚未登录或发送消息。");
            }
            if ((publishing.Count > 0 || steps.Any(step => step.Action == "qtrade.negotiation"))
                && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("STC_UI_INTAKE_TOKEN")))
            {
                throw new InvalidOperationException("QT 用例缺少共享接入凭据。请用 autotest.cmd restart -QTrade 自动加载 ATS_ROOT/control 中已有配置；尚未登录或发送消息。");
            }
            foreach (TestStep step in publishing)
            {
                string receiver = (string)step.Parameters["receiverLogin"];
                if (!string.Equals(user, receiver, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("QT 接收验证要求登录 " + receiver + "，当前账号为 " + user + "。请修改测试账号；尚未登录或发送消息。");
                }
            }
        }

        private static string Quote(string path)
        {
            if (path.Contains("\"")) throw new ArgumentException("路径不能包含双引号");
            return "\"" + path + "\"";
        }
    }
}
