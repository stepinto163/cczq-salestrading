using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SalesTrading.UiTests
{
    // 每例独立账号、客户端缓存和日志；父管理器统一负责停止与进程看门狗。
    internal sealed class ObserverSession : IDisposable
    {
        private readonly Process process;
        public string DirectoryPath { get; }

        public ObserverSession(string runtime, string caseFile, string caseDirectory, string user, string password, string security)
        {
            DirectoryPath = Path.Combine(caseDirectory, "observer");
            Directory.CreateDirectory(DirectoryPath);
            string executable = Path.Combine(runtime, "UiObserverWorker-" + Guid.NewGuid().ToString("N") + ".exe");
            File.Copy(Path.Combine(runtime, "UiScenarioWorker.exe"), executable, true);
            var config = new XmlDocument();
            config.Load(Path.Combine(runtime, "UiScenarioWorker.exe.config"));
            Set(config, "DataPath", Path.Combine(DirectoryPath, "client-state"));
            Set(config, "LogPath", Path.Combine(DirectoryPath, "logs", "STC_"));
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "client-state"));
            Directory.CreateDirectory(Path.Combine(DirectoryPath, "logs"));
            config.Save(executable + ".config");
            var start = new ProcessStartInfo(executable, "\"" + caseFile + "\" \"" + DirectoryPath + "\"")
            {
                WorkingDirectory = runtime, UseShellExecute = false, CreateNoWindow = true
            };
            start.EnvironmentVariables["STC_UI_USER"] = user;
            start.EnvironmentVariables["STC_UI_PASSWORD"] = password;
            start.EnvironmentVariables["STC_UI_SECURITY"] = security;
            start.EnvironmentVariables["STC_UI_REALTIME_ROLE"] = "observer";
            start.EnvironmentVariables["STC_UI_REALTIME_ROOT"] = caseDirectory;
            start.EnvironmentVariables["STC_UI_PARENT_PID"] = Process.GetCurrentProcess().Id.ToString();
            start.EnvironmentVariables.Remove("STC_UI_INTAKE_TOKEN");
            process = Process.Start(start);
        }

        private static void Set(XmlDocument config, string key, string value)
        {
            ((XmlElement)config.SelectSingleNode("/configuration/appSettings/add[@key='" + key + "']")).SetAttribute("value", value);
        }

        public void Cancel()
        {
            File.WriteAllText(Path.Combine(DirectoryPath, "cancel"), "stop");
        }

        public async Task<JObject> CompleteAsync()
        {
            var deadline = DateTime.UtcNow.AddSeconds(15);
            while (!process.HasExited && DateTime.UtcNow < deadline)
            {
                await Task.Delay(100);
            }
            string result = Path.Combine(DirectoryPath, "result.json");
            if (!process.HasExited || !File.Exists(result))
            {
                return new JObject { ["status"] = "FAILED", ["cleanupPassed"] = false,
                    ["reason"] = "观察账号未完成或没有报告，请检查 observer 日志" };
            }
            return JObject.Parse(File.ReadAllText(result));
        }

        public void Dispose()
        {
            if (!process.HasExited)
            {
                Cancel();
                if (!process.WaitForExit(10000)) process.Kill();
            }
            process.Dispose();
        }
    }
}
