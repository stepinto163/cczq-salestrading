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
        public bool AdvancedMode { get; set; }
        public string ObserverUser { get; set; }
        public string ObserverPassword { get; set; }
        private ObserverSession currentObserver;

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
            currentObserver?.Cancel();
            if (CurrentDirectory != null) File.WriteAllText(Path.Combine(CurrentDirectory, "cancel"), "stop");
        }

        public async Task<bool> RunAsync(IEnumerable<CaseDefinition> selected, string configFile, string user, string password, string securityId, string output)
        {
            List<CaseDefinition> cases = selected.ToList();
            if (cases.Any(item => item.Steps.Any(step => step.Action == "realtime.lifecycle"))
                && (!AdvancedMode || string.IsNullOrWhiteSpace(ObserverUser) || string.IsNullOrEmpty(ObserverPassword)
                    || string.Equals(user, ObserverUser, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("双账号实时用例需要启用高级模式，填写不同的观察账号及密码。");
            }
            if (cases.Count == 0) throw new InvalidOperationException("请勾选用例。");
            foreach (CaseDefinition definition in cases) definition.Validate();
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
                        start.EnvironmentVariables.Remove("STC_UI_REALTIME_ROLE");
                        start.EnvironmentVariables["STC_UI_REALTIME_ROOT"] = CurrentDirectory;
                        bool needsIntake = definition.Steps.Any(step => step.Action == "qtrade.publish"
                            || step.Action == "qtrade.negotiation");
                        start.EnvironmentVariables.Remove("STC_UI_INTAKE_TOKEN");
                        if (needsIntake)
                        {
                            start.EnvironmentVariables["STC_UI_INTAKE_TOKEN"] = QTradeTestConfiguration.LoadToken(
                                configFile, Environment.GetEnvironmentVariable("ATS_ROOT"));
                        }
                        using (ObserverSession observer = definition.Steps.Any(step => step.Action == "realtime.lifecycle")
                            ? new ObserverSession(runtime, caseFile, CurrentDirectory, ObserverUser, ObserverPassword, securityId) : null)
                        using (StreamWriter console = new StreamWriter(Path.Combine(CurrentDirectory, "console.log"), false, Encoding.UTF8))
                        using (Process process = new Process { StartInfo = start })
                        {
                            currentObserver = observer;
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
                            int observerEventCount = 0;
                            var observerWaitBudget = new OperationWaitBudget();
                            bool killed = false;
                            int timeoutSeconds = Math.Min(600, 120 + definition.Steps.Count * 25);
                            while (!process.HasExited)
                            {
                                ReadEvents(ref eventCount, CurrentDirectory, operationWaitBudget, false);
                                if (observer != null)
                                {
                                    ReadEvents(ref observerEventCount, observer.DirectoryPath, observerWaitBudget, true);
                                }
                                if (StopRequested && stopTime == null)
                                {
                                    stopTime = DateTime.UtcNow;
                                    File.WriteAllText(Path.Combine(CurrentDirectory, "cancel"), "stop");
                                }
                                if (elapsed.ElapsedMilliseconds - operationWaitBudget.ExcludedMilliseconds - observerWaitBudget.ExcludedMilliseconds > timeoutSeconds * 1000L || (stopTime != null && DateTime.UtcNow - stopTime > TimeSpan.FromSeconds(30)))
                                {
                                    // This is the single worker started above, never an unrelated STC process.
                                    process.Kill();
                                    killed = true;
                                    break;
                                }
                                await Task.Delay(200);
                            }
                            process.WaitForExit();
                            ReadEvents(ref eventCount, CurrentDirectory, operationWaitBudget, false);
                            if (observer != null)
                            {
                                ReadEvents(ref observerEventCount, observer.DirectoryPath, observerWaitBudget, true);
                            }
                            string resultFile = Path.Combine(CurrentDirectory, "result.json");
                            JObject result = File.Exists(resultFile) ? JObject.Parse(File.ReadAllText(resultFile)) : new JObject
                            {
                                ["caseId"] = definition.Id, ["name"] = definition.Name, ["status"] = "FAILED",
                                ["elapsedMillis"] = elapsed.ElapsedMilliseconds, ["cleanupPassed"] = false,
                                ["reason"] = killed ? "超时或停止宽限期结束；清理未确认，请检查 owned-quotes.json" : "进程提前退出，退出码 " + process.ExitCode + "；清理未确认"
                            };
                            if (process.ExitCode != 0 && (string)result["status"] == "PASSED") result["status"] = "FAILED";
                            if (observer != null)
                            {
                                if ((string)result["status"] != "PASSED") observer.Cancel();
                                JObject observation = await observer.CompleteAsync();
                                result["observer"] = observation;
                                if ((string)observation["status"] != "PASSED") result["status"] = "FAILED";
                                if ((bool?)observation["cleanupPassed"] != true) result["cleanupPassed"] = false;
                            }
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
                    currentObserver = null;
                    CurrentDirectory = null;
                    File.WriteAllText(Path.Combine(OutputDirectory, "summary.json"), new JArray(results).ToString());
                    WriteJUnit(results);
                }
            }
            return results.Count == cases.Count && results.All(result => (string)result["status"] == "PASSED");
        }

        private void ReadEvents(ref int count, string directory, OperationWaitBudget waitBudget, bool observer)
        {
            string file = Path.Combine(directory, "events.jsonl");
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
                        waitBudget.Begin();
                    }
                    else if ((string)entry["state"] == "INTERVAL_DONE")
                    {
                        waitBudget.Complete((long?)entry["waitMillis"] ?? 0);
                    }
                    if (observer)
                    {
                        entry["step"] = "观察账号 · " + (string)entry["step"];
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

        private static string Quote(string path)
        {
            if (path.Contains("\"")) throw new ArgumentException("路径不能包含双引号");
            return "\"" + path + "\"";
        }
    }
}
